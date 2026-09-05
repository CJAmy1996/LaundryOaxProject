import { Component, HostListener, OnDestroy, OnInit, provideZonelessChangeDetection, signal } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserManager, WebStorageStateStore, InMemoryWebStorage } from 'oidc-client-ts';

interface Machine { tenantId: string; deviceId: string; name: string; location: string; kind: string; phase: string; progress: number; temperatureC: number; moisturePercent: number; turbidityNtu: number; lastSequence: number; lastSeenUtc: string; estimatedMinutes: number; extraRinses: number; }
interface Notice { id: string; deviceId: string; message: string; createdUtc: string; read: boolean; }
interface EventRow { eventId: string; deviceId: string; sequence: number; receivedUtc: string; processedUtc: string | null; }
interface Snapshot { tenant: string; demo: boolean; serverTime: string; machines: Machine[]; received: number; pending: number; notifications: Notice[]; events: EventRow[]; }
@Component({ selector: 'app-root', standalone: true, imports: [CommonModule, FormsModule], templateUrl: './app.html' })
class App implements OnInit, OnDestroy {
  data = signal<Snapshot | null>(null); error = signal(''); busy = signal(false); connected = signal(false);
  page = signal('Overview'); selected = signal<Machine | null>(null); showNotifications = signal(false); showAdd = signal(false);
  search = ''; location = 'All locations'; status = 'all';
  draft = { deviceId: '', name: '', kind: 'washer', location: 'Centro' };
  nav = ['Overview', 'Machines', 'Event stream', 'Reliability']; timer?: ReturnType<typeof setInterval>;
  auth?: UserManager; token = ''; loading = false; authRequired = signal(false);
  async ngOnInit() {
    try {
      const response = await fetch('/api/config'); if (!response.ok) throw new Error('Unable to read platform configuration.');
      const config = await response.json();
      if (!config.demo) {
        if (!config.authority || !config.clientId) throw new Error('Configure the OIDC authority and client ID to sign in.');
        this.auth = new UserManager({ authority: config.authority, client_id: config.clientId,
          redirect_uri: window.location.origin + '/', response_type: 'code', scope: config.scope || 'openid profile',
          userStore: new WebStorageStateStore({ store: new InMemoryWebStorage() }), automaticSilentRenew: false });
        if (new URLSearchParams(location.search).has('code')) {
          const user = await this.auth.signinRedirectCallback(); this.token = user.access_token;
          history.replaceState({}, '', '/');
        } else { this.authRequired.set(true); return; }
      }
      await this.refresh(); this.timer = setInterval(() => this.refresh(), 3000);
    } catch (e) { this.fail(e); }
  }
  ngOnDestroy() { if (this.timer) clearInterval(this.timer); }
  @HostListener('document:keydown', ['$event'])
  keyboard(event: KeyboardEvent) {
    const dialog = document.querySelector<HTMLElement>('[role="dialog"]');
    if (!dialog) return;
    if (event.key === 'Escape') { this.selected.set(null); this.showAdd.set(false); this.showNotifications.set(false); }
    if (event.key === 'Tab') {
      const focusable = [...dialog.querySelectorAll<HTMLElement>('button:not([disabled]),input,select,a[href]')];
      const first = focusable[0], last = focusable[focusable.length - 1];
      if (!first) return;
      if (!dialog.contains(document.activeElement) || (!event.shiftKey && document.activeElement === last)) { event.preventDefault(); first.focus(); }
      else if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    }
  }
  async signIn() { try { await this.auth?.signinRedirect(); } catch(e) { this.fail(e); } }
  async request(path: string, init: RequestInit = {}) {
    const response = await fetch(path, { ...init, headers: { 'Content-Type': 'application/json', ...(this.token ? { Authorization: 'Bearer ' + this.token } : {}), ...init.headers } });
    if (!response.ok) {
      if (response.status === 401) this.authRequired.set(true);
      const body = await response.json().catch(() => ({}));
      throw new Error(body.error || (response.status === 403 ? 'Your role does not allow this action.' : `Request failed (${response.status}).`));
    }
    return response;
  }
  async refresh() {
    if (this.loading || this.authRequired()) return;
    this.loading = true;
    try { const r = await this.request('/api/fleet/snapshot'); const d = await r.json() as Snapshot; this.data.set(d); this.connected.set(true);
      if (this.selected()) this.selected.set(d.machines.find(m => m.deviceId === this.selected()!.deviceId) || null);
    } catch (e) { this.connected.set(false); this.fail(e); } finally { this.loading = false; }
  }
  fail(e: unknown) { this.error.set(e instanceof Error ? e.message : 'Something went wrong. Please try again.'); }
  machines() { return (this.data()?.machines || []).filter(m => (this.location === 'All locations' || m.location === this.location) &&
    (this.status === 'all' || (this.status === 'active' ? this.active(m) : m.phase === this.status)) &&
    (m.name + m.deviceId + m.location).toLowerCase().includes(this.search.toLowerCase())); }
  locations() { return [...new Set(this.data()?.machines.map(m => m.location) || [])]; }
  active(m: Machine) { return !['idle', 'complete', 'fault'].includes(m.phase); }
  activeCount() { return this.data()?.machines.filter(m => this.active(m)).length || 0; }
  completeCount() { return this.data()?.machines.filter(m => m.phase === 'complete').length || 0; }
  offline(m: Machine) { return !m.lastSequence || Date.now() - new Date(m.lastSeenUtc + (m.lastSeenUtc.endsWith('Z') ? '' : 'Z')).getTime() > 120000; }
  onlineCount() { return this.data()?.machines.filter(m => !this.offline(m)).length || 0; }
  unread() { return this.data()?.notifications.filter(n => !n.read).length || 0; }
  async tick() { this.busy.set(true); try { await this.request('/api/demo/tick', { method: 'POST' }); await new Promise(r => setTimeout(r, 650)); await this.refresh(); } catch (e) { this.fail(e); } finally { this.busy.set(false); } }
  async markRead(n: Notice) { try { await this.request(`/api/fleet/notifications/${n.id}/read`, { method: 'POST' }); await this.refresh(); } catch (e) { this.fail(e); } }
  async register() { this.busy.set(true); try { await this.request('/api/fleet/machines', { method: 'POST', body: JSON.stringify(this.draft) }); this.showAdd.set(false); this.draft = { deviceId: '', name: '', kind: 'washer', location: 'Centro' }; await this.refresh(); } catch (e) { this.fail(e); } finally { this.busy.set(false); } }
  exportEvents() {
    const rows = ['event_id,device,sequence,received_utc,processed_utc', ...(this.data()?.events || []).map(e => [e.eventId,e.deviceId,e.sequence,e.receivedUtc,e.processedUtc || ''].map(v => '"' + String(v).replace(/"/g,'""').replace(/^[=+@-]/, "'") + '"').join(','))];
    const url = URL.createObjectURL(new Blob([rows.join('\n')], { type: 'text/csv' })); const a = document.createElement('a'); a.href = url; a.download = 'laundryoax-events.csv'; a.click(); URL.revokeObjectURL(url);
  }
}
bootstrapApplication(App, { providers: [provideZonelessChangeDetection()] }).catch(console.error);
