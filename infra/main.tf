terraform {
  required_version = ">= 1.6.0"
  required_providers {
    aws = { source = "hashicorp/aws", version = "~> 5.0" }

  }
}
provider "aws" {
  region = var.region
}
variable "region" {
  type    = string
  default = "us-east-1"
}
variable "name" {
  type    = string
  default = "laundryoax"
}
variable "devices" {
  description = "Provisioned device certificates. Map keys become MQTT client/thing IDs."
  type        = map(object({ tenant = string, device = string, certificate_arn = string }))
  default     = {}
}
data "aws_caller_identity" "current" {}
data "aws_partition" "current" {}
locals {
  account    = data.aws_caller_identity.current.account_id
  arn_prefix = "arn:${data.aws_partition.current.partition}"
  rule_name  = "${var.name}_telemetry"
}
resource "aws_kms_key" "telemetry" {
  description             = "Laundry telemetry queue encryption"
  enable_key_rotation     = true
  deletion_window_in_days = 30
}
resource "aws_sqs_queue" "dead_letter" {
  name                      = "${var.name}-telemetry-dlq"
  kms_master_key_id         = aws_kms_key.telemetry.arn
  message_retention_seconds = 1209600
}
resource "aws_sqs_queue" "telemetry" {
  name                       = "${var.name}-telemetry"
  kms_master_key_id          = aws_kms_key.telemetry.arn
  visibility_timeout_seconds = 60
  receive_wait_time_seconds  = 20
  message_retention_seconds  = 1209600
  redrive_policy             = jsonencode({ deadLetterTargetArn = aws_sqs_queue.dead_letter.arn, maxReceiveCount = 5 })
}
resource "aws_sqs_queue_redrive_allow_policy" "dlq" {
  queue_url            = aws_sqs_queue.dead_letter.id
  redrive_allow_policy = jsonencode({ redrivePermission = "byQueue", sourceQueueArns = [aws_sqs_queue.telemetry.arn] })
}
resource "aws_sqs_queue_policy" "tls" {
  queue_url = aws_sqs_queue.telemetry.id
  policy    = jsonencode({ Version = "2012-10-17", Statement = [{ Effect = "Deny", Principal = "*", Action = "sqs:*", Resource = aws_sqs_queue.telemetry.arn, Condition = { Bool = { "aws:SecureTransport" = "false" } } }] })
}
resource "aws_iam_role" "iot" {
  name               = "${var.name}-iot-ingestion"
  assume_role_policy = jsonencode({ Version = "2012-10-17", Statement = [{ Effect = "Allow", Principal = { Service = "iot.amazonaws.com" }, Action = "sts:AssumeRole", Condition = { StringEquals = { "aws:SourceAccount" = local.account }, ArnLike = { "aws:SourceArn" = "${local.arn_prefix}:iot:${var.region}:${local.account}:rule/${local.rule_name}" } } }] })
}
resource "aws_iam_role_policy" "iot" {
  role = aws_iam_role.iot.id
  policy = jsonencode({ Version = "2012-10-17", Statement = [
    { Effect = "Allow", Action = ["sqs:SendMessage"], Resource = aws_sqs_queue.telemetry.arn },
    { Effect = "Allow", Action = ["kms:Decrypt", "kms:GenerateDataKey"], Resource = aws_kms_key.telemetry.arn }
  ] })
}
resource "aws_iot_topic_rule" "ingestion" {
  name        = local.rule_name
  enabled     = true
  sql_version = "2016-03-23"
  # Identity is reconstructed from the certificate-authorized topic. Never SELECT *.
  sql = "SELECT eventId AS EventId, topic(2) AS TenantId, topic(4) AS DeviceId, sequence AS Sequence, observedAt AS ObservedAt, phase AS Phase, progress AS Progress, temperatureC AS TemperatureC, moisturePercent AS MoisturePercent, turbidityNtu AS TurbidityNtu FROM 'laundry/+/devices/+/telemetry'"
  sqs {
    queue_url  = aws_sqs_queue.telemetry.id
    role_arn   = aws_iam_role.iot.arn
    use_base64 = false

  }
  depends_on = [aws_iam_role_policy.iot]
}
resource "aws_iot_thing" "device" {
  for_each   = var.devices
  name       = each.key
  attributes = { tenant = each.value.tenant, device = each.value.device }
}
resource "aws_iot_policy" "device" {
  for_each = var.devices
  name     = "${var.name}-${each.key}"
  policy = jsonencode({ Version = "2012-10-17", Statement = [
    { Effect = "Allow", Action = ["iot:Connect"], Resource = "${local.arn_prefix}:iot:${var.region}:${local.account}:client/${each.key}" },
    { Effect = "Allow", Action = ["iot:Publish"], Resource = "${local.arn_prefix}:iot:${var.region}:${local.account}:topic/laundry/${each.value.tenant}/devices/${each.value.device}/telemetry" }
  ] })
}
resource "aws_iot_policy_attachment" "device" {
  for_each = var.devices
  policy   = aws_iot_policy.device[each.key].name
  target   = each.value.certificate_arn
}
resource "aws_iot_thing_principal_attachment" "device" {
  for_each  = var.devices
  thing     = aws_iot_thing.device[each.key].name
  principal = each.value.certificate_arn
}
resource "aws_cloudwatch_metric_alarm" "dead_letters" {
  alarm_name          = "${var.name}-dead-letters"
  namespace           = "AWS/SQS"
  metric_name         = "ApproximateNumberOfMessagesVisible"
  dimensions          = { QueueName = aws_sqs_queue.dead_letter.name }
  statistic           = "Maximum"
  period              = 60
  evaluation_periods  = 1
  threshold           = 0
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "notBreaching"
  alarm_actions       = var.alarm_topic_arns
}
variable "alarm_topic_arns" {
  type    = list(string)
  default = []
}
resource "aws_cloudwatch_metric_alarm" "backlog" {
  alarm_name          = "${var.name}-telemetry-age"
  namespace           = "AWS/SQS"
  metric_name         = "ApproximateAgeOfOldestMessage"
  dimensions          = { QueueName = aws_sqs_queue.telemetry.name }
  statistic           = "Maximum"
  period              = 60
  evaluation_periods  = 3
  threshold           = 120
  comparison_operator = "GreaterThanThreshold"
  treat_missing_data  = "notBreaching"
  alarm_actions       = var.alarm_topic_arns
}
output "queue_url" {
  value = aws_sqs_queue.telemetry.id
}
output "queue_arn" {
  value = aws_sqs_queue.telemetry.arn
}
output "kms_key_arn" {
  value = aws_kms_key.telemetry.arn
}
