// Lớp gọi API v1 (JSON). Mọi màn hình React dùng chung → dễ soi API.
const BASE = '/api/v1';
export let lastApi = { path: '—' };
export async function api(path, opts) {
  lastApi.path = (opts?.method || 'GET') + ' ' + BASE + path;
  const r = await fetch(BASE + path, { headers: { 'Content-Type': 'application/json' }, ...opts });
  if (!r.ok) { let e; try { e = await r.json(); } catch { e = { error: r.status }; } throw new Error(e.error || e.msg || ('HTTP ' + r.status)); }
  return r.status === 204 ? null : r.json();
}
export const fmtM = n => (n || 0).toLocaleString('vi-VN') + 'đ';
