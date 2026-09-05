# Optional deployment into an existing VPC and shared SQL Server installation.
variable "deployment" {
  type = object({ vpc_id = string, private_subnet_ids = list(string), public_subnet_ids = list(string),
    image = string, certificate_arn = string, database_secret_arn = string, authority = string,
  audience = string, client_id = string, desired_count = number, proxy_network = string, proxy_prefix_length = number })
  default = null
}
resource "aws_ecr_repository" "api" {
  name                 = var.name
  image_tag_mutability = "IMMUTABLE"
  image_scanning_configuration { scan_on_push = true }
  encryption_configuration { encryption_type = "AES256" }
}
resource "aws_cloudwatch_log_group" "api" {
  name              = "/ecs/${var.name}"
  retention_in_days = 30
}
resource "aws_ecs_cluster" "fleet" {
  name = var.name
  setting {
    name  = "containerInsights"
    value = "enabled"
  }
}
resource "aws_iam_role" "task" {
  name               = "${var.name}-task"
  assume_role_policy = jsonencode({ Version = "2012-10-17", Statement = [{ Effect = "Allow", Principal = { Service = "ecs-tasks.amazonaws.com" }, Action = "sts:AssumeRole" }] })
}
resource "aws_iam_role_policy" "consume" {
  role = aws_iam_role.task.id
  policy = jsonencode({ Version = "2012-10-17", Statement = [
    { Effect = "Allow", Action = ["sqs:ReceiveMessage", "sqs:DeleteMessage", "sqs:GetQueueAttributes"], Resource = aws_sqs_queue.telemetry.arn },
    { Effect = "Allow", Action = ["kms:Decrypt"], Resource = aws_kms_key.telemetry.arn }
  ] })
}
resource "aws_iam_role" "execution" {
  name               = "${var.name}-execution"
  assume_role_policy = aws_iam_role.task.assume_role_policy
}
resource "aws_iam_role_policy_attachment" "execution" {
  role       = aws_iam_role.execution.name
  policy_arn = "${local.arn_prefix}:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}
resource "aws_iam_role_policy" "secret" {
  count  = var.deployment == null ? 0 : 1
  role   = aws_iam_role.execution.id
  policy = jsonencode({ Version = "2012-10-17", Statement = [{ Effect = "Allow", Action = ["secretsmanager:GetSecretValue"], Resource = var.deployment.database_secret_arn }] })
}
resource "aws_security_group" "alb" {
  count  = var.deployment == null ? 0 : 1
  name   = "${var.name}-alb"
  vpc_id = var.deployment.vpc_id
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port       = 8080
    to_port         = 8080
    protocol        = "tcp"
    security_groups = [aws_security_group.api[0].id]
  }
}
resource "aws_security_group" "api" {
  count  = var.deployment == null ? 0 : 1
  name   = "${var.name}-api"
  vpc_id = var.deployment.vpc_id
  egress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  egress {
    from_port   = 1433
    to_port     = 1433
    protocol    = "tcp"
    cidr_blocks = ["10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16"]
  }
}
resource "aws_security_group_rule" "api_ingress" {
  count                    = var.deployment == null ? 0 : 1
  type                     = "ingress"
  from_port                = 8080
  to_port                  = 8080
  protocol                 = "tcp"
  security_group_id        = aws_security_group.api[0].id
  source_security_group_id = aws_security_group.alb[0].id
}
resource "aws_lb" "api" {
  count                      = var.deployment == null ? 0 : 1
  name                       = var.name
  load_balancer_type         = "application"
  subnets                    = var.deployment.public_subnet_ids
  security_groups            = [aws_security_group.alb[0].id]
  drop_invalid_header_fields = true
}
resource "aws_lb_target_group" "api" {
  count       = var.deployment == null ? 0 : 1
  name        = var.name
  port        = 8080
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = var.deployment.vpc_id
  health_check {
    path    = "/health/ready"
    matcher = "200"
  }
}
resource "aws_lb_listener" "https" {
  count             = var.deployment == null ? 0 : 1
  load_balancer_arn = aws_lb.api[0].arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = var.deployment.certificate_arn
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api[0].arn
  }
}
resource "aws_ecs_task_definition" "api" {
  count                    = var.deployment == null ? 0 : 1
  family                   = var.name
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "512"
  memory                   = "1024"
  task_role_arn            = aws_iam_role.task.arn
  execution_role_arn       = aws_iam_role.execution.arn
  container_definitions = jsonencode([{
    name         = "api", image = var.deployment.image, essential = true, readonlyRootFilesystem = true,
    portMappings = [{ containerPort = 8080 }],
    environment = [
      { name = "ASPNETCORE_ENVIRONMENT", value = "Production" },
      { name = "Auth__Authority", value = var.deployment.authority },
      { name = "Auth__Audience", value = var.deployment.audience },
      { name = "Auth__ClientId", value = var.deployment.client_id },
      { name = "Fleet__Provider", value = "SqlServer" },
      { name = "Proxy__KnownNetwork", value = var.deployment.proxy_network },
      { name = "Proxy__PrefixLength", value = tostring(var.deployment.proxy_prefix_length) },
      { name = "Aws__QueueUrl", value = aws_sqs_queue.telemetry.id }
    ],
    secrets          = [{ name = "ConnectionStrings__Fleet", valueFrom = var.deployment.database_secret_arn }],
    logConfiguration = { logDriver = "awslogs", options = { awslogs-group = aws_cloudwatch_log_group.api.name, awslogs-region = var.region, awslogs-stream-prefix = "api" } }
  }])
}
resource "aws_ecs_service" "api" {
  count           = var.deployment == null ? 0 : 1
  name            = var.name
  cluster         = aws_ecs_cluster.fleet.id
  task_definition = aws_ecs_task_definition.api[0].arn
  desired_count   = var.deployment.desired_count
  launch_type     = "FARGATE"
  network_configuration {
    subnets          = var.deployment.private_subnet_ids
    security_groups  = [aws_security_group.api[0].id]
    assign_public_ip = false
  }
  load_balancer {
    target_group_arn = aws_lb_target_group.api[0].arn
    container_name   = "api"
    container_port   = 8080
  }
  deployment_circuit_breaker {
    enable   = true
    rollback = true
  }
  depends_on = [aws_lb_listener.https]
}
output "repository_url" {
  value = aws_ecr_repository.api.repository_url
}
output "load_balancer_dns" {
  value = try(aws_lb.api[0].dns_name, null)
}
