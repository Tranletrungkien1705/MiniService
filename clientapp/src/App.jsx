import React, { useState, useEffect, useCallback } from 'react'
import { Routes, Route, NavLink, Link, useParams, useNavigate, useSearchParams } from 'react-router-dom'
import { api, fmtM, lastApi } from './api.js'

// ---- Toast dùng chung ----
function useToast() {
  const [msg, setMsg] = useState(null)
  const show = useCallback(m => { setMsg(m); setTimeout(() => setMsg(null), 3800) }, [])
  const node = msg && <div style={{ position: 'fixed', bottom: 20, right: 20, background: '#111', color: '#fff', padding: '10px 16px', borderRadius: 8, zIndex: 9999, fontSize: 14 }}>{msg}</div>
  return [show, node]
}
const ToastCtx = React.createContext(() => { })

// ---- Hook tải dữ liệu ----
function useApi(path, deps = []) {
  const [data, setData] = useState(null), [err, setErr] = useState(null), [tick, setTick] = useState(0)
  useEffect(() => { let ok = true; setErr(null); api(path).then(d => ok && setData(d)).catch(e => ok && setErr(e.message)); return () => ok = false }, [path, tick, ...deps])
  return { data, err, reload: () => setTick(t => t + 1) }
}

const Stat = ({ t, v, c, sm }) => (
  <div className="col-6 col-lg-3"><div className="card"><div className="card-body">
    <div className="text-muted small">{t}</div><div style={{ fontSize: sm ? 18 : 26, fontWeight: 800, color: c }}>{v}</div>
  </div></div></div>)

// ================= Screens =================
function Dashboard() {
  const { data: d } = useApi('/dashboard')
  if (!d) return <div>Đang tải…</div>
  return <>
    <div className="row g-3 mb-3">
      <Stat t="RO đang mở" v={d.openRO} c="#2563eb" /><Stat t="Đang sửa" v={d.inGarage} c="#f59e0b" />
      <Stat t="Xong hôm nay" v={d.doneToday} c="#16a34a" /><Stat t="Doanh thu tháng" v={fmtM(d.revenueMonth)} c="#0f766e" sm />
    </div>
    <div className="card"><div className="card-body">
      <div className="d-flex justify-content-between"><h6 className="fw-bold">RO theo trạng thái</h6><Link to="/createro" className="btn btn-sm btn-primary"><i className="bi bi-plus" /> Tiếp nhận xe</Link></div>
      {(d.byStatus || []).map((s, i) => <div key={i} className="d-flex justify-content-between small border-bottom py-1"><span>{s.statusText}</span><b>{s.count}</b></div>)}
    </div></div>
  </>
}

function ROs() {
  const { data: rows } = useApi('/ros')
  return <div className="card"><div className="card-body">
    <div className="text-end mb-2"><Link to="/createro" className="btn btn-sm btn-primary"><i className="bi bi-plus" /> Tiếp nhận xe</Link></div>
    <table className="table table-hover align-middle mb-0"><thead><tr><th>Mã</th><th>Biển số</th><th>Khách</th><th className="text-end">Tổng</th><th>Trạng thái</th><th>HĐĐT</th></tr></thead>
      <tbody>{(rows || []).map(r => <tr key={r.id} style={{ cursor: 'pointer' }} onClick={() => location.hash = '#/ro/' + r.id}>
        <td className="fw-semibold">{r.code}</td><td>{r.plate}</td><td className="small">{r.customer}</td><td className="text-end">{fmtM(r.total)}</td>
        <td><span className="badge bg-secondary">{r.statusText}</span></td><td>{r.eInvoice ? <span className="badge bg-success">{r.eInvoice}</span> : <span className="text-muted small">—</span>}</td></tr>)}
        {rows && !rows.length && <tr><td colSpan={6} className="text-center text-muted py-3">Chưa có RO</td></tr>}</tbody></table>
  </div></div>
}

function CreateRO() {
  const { data: cars } = useApi('/cars')
  const [f, setF] = useState({ carId: '', odometer: 0, technician: '', intakeNote: '' })
  const nav = useNavigate(); const toast = React.useContext(ToastCtx)
  useEffect(() => { if (cars?.length && !f.carId) setF(s => ({ ...s, carId: cars[0].id })) }, [cars])
  const submit = async () => {
    try { const r = await api('/ros', { method: 'POST', body: JSON.stringify({ ...f, carId: +f.carId }) }); toast('Đã tiếp nhận xe, RO #' + r.id); nav('/ro/' + r.id) }
    catch (e) { toast('❌ ' + e.message) }
  }
  return <div className="card" style={{ maxWidth: 640 }}><div className="card-body">
    <div className="mb-2"><label className="form-label small text-muted">Chọn xe *</label>
      <select className="form-select" value={f.carId} onChange={e => setF({ ...f, carId: e.target.value })}>
        {(cars || []).map(c => <option key={c.id} value={c.id}>{c.plate} — {c.model} ({c.customerName || ''})</option>)}</select>
      {cars && !cars.length && <div className="text-danger small mt-1">Chưa có xe — <Link to="/cars">thêm xe</Link> trước.</div>}</div>
    <div className="row g-2"><div className="col-4"><label className="form-label small text-muted">Số km</label><input type="number" className="form-control" value={f.odometer} onChange={e => setF({ ...f, odometer: +e.target.value })} /></div>
      <div className="col-8"><label className="form-label small text-muted">Thợ phụ trách</label><input className="form-control" value={f.technician} onChange={e => setF({ ...f, technician: e.target.value })} /></div></div>
    <div className="mb-3 mt-2"><label className="form-label small text-muted">Ghi nhận tình trạng</label><textarea className="form-control" rows={2} value={f.intakeNote} onChange={e => setF({ ...f, intakeNote: e.target.value })} /></div>
    <button className="btn btn-primary" onClick={submit} disabled={!f.carId}><i className="bi bi-check-lg me-1" />Tiếp nhận</button>
  </div></div>
}

function RODetail() {
  const { id } = useParams()
  const { data: r, reload } = useApi('/ros/' + id)
  const [ln, setLn] = useState({ type: 0, name: '', quantity: 1, unitPrice: 0 })
  const toast = React.useContext(ToastCtx)
  const run = async (fn) => { try { await fn() } catch (e) { toast('❌ ' + e.message) } }
  if (!r) return <div>Đang tải…</div>
  const addLine = () => run(async () => { if (!ln.name.trim()) return toast('Nhập nội dung'); await api(`/ros/${id}/lines`, { method: 'POST', body: JSON.stringify({ ...ln, quantity: +ln.quantity, unitPrice: +ln.unitPrice }) }); setLn({ type: 0, name: '', quantity: 1, unitPrice: 0 }); reload() })
  const delLine = lid => run(async () => { await api(`/ros/${id}/lines/${lid}`, { method: 'DELETE' }); reload() })
  const trans = to => run(async () => { const x = await api(`/ros/${id}/transition`, { method: 'POST', body: JSON.stringify({ to }) }); toast(x.msg); reload() })
  const issueInv = () => run(async () => { toast('Đang đẩy HĐĐT…'); const x = await api(`/ros/${id}/einvoice`, { method: 'POST' }); toast(x.msg); reload() })
  const settle = () => run(async () => { const m = prompt('Quyết toán — 0=Tiền mặt,1=Chuyển khoản,2=Thẻ', '0'); if (m === null) return; const x = await api('/inventory/settle', { method: 'POST', body: JSON.stringify({ roId: +id, method: +m, note: null }) }); toast(x.msg); reload() })
  return <div className="row g-3">
    <div className="col-lg-8"><div className="card"><div className="card-body">
      <h5 className="fw-bold">{r.code} <span className="badge bg-secondary">{r.statusText}</span></h5>
      <div className="text-muted small mb-2">{r.car.plate} · {r.car.model} · {r.odometer || 0} km · KH: {r.customer.name} ({r.customer.phone || ''})</div>
      <table className="table table-sm"><thead><tr><th>Loại</th><th>Nội dung</th><th className="text-end">SL</th><th className="text-end">Đơn giá</th><th className="text-end">Thành tiền</th><th /></tr></thead>
        <tbody>{r.lines.map(l => <tr key={l.id}><td>{l.type === 0 ? <span className="badge bg-info">Công</span> : <span className="badge bg-secondary">PT</span>}</td><td>{l.name}</td><td className="text-end">{l.quantity}</td><td className="text-end">{fmtM(l.unitPrice)}</td><td className="text-end">{fmtM(l.amount)}</td><td className="text-end"><i className="bi bi-x text-danger" style={{ cursor: 'pointer' }} onClick={() => delLine(l.id)} /></td></tr>)}
          {!r.lines.length && <tr><td colSpan={6} className="text-muted">Chưa có dòng</td></tr>}</tbody>
        <tfoot><tr className="fw-bold"><td colSpan={4} className="text-end">TỔNG</td><td className="text-end" style={{ color: '#1d4ed8' }}>{fmtM(r.total)}</td><td /></tr></tfoot></table>
      <div className="border rounded p-2 bg-light"><div className="small fw-bold mb-1">Thêm dòng chi phí</div><div className="row g-1">
        <div className="col-2"><select className="form-select form-select-sm" value={ln.type} onChange={e => setLn({ ...ln, type: +e.target.value })}><option value={0}>Công</option><option value={1}>Phụ tùng</option></select></div>
        <div className="col-4"><input className="form-control form-control-sm" placeholder="Nội dung" value={ln.name} onChange={e => setLn({ ...ln, name: e.target.value })} /></div>
        <div className="col-2"><input type="number" className="form-control form-control-sm" value={ln.quantity} onChange={e => setLn({ ...ln, quantity: e.target.value })} /></div>
        <div className="col-2"><input type="number" className="form-control form-control-sm" placeholder="Đơn giá" value={ln.unitPrice} onChange={e => setLn({ ...ln, unitPrice: e.target.value })} /></div>
        <div className="col-2"><button className="btn btn-sm btn-primary w-100" onClick={addLine}>Thêm</button></div></div></div>
    </div></div></div>
    <div className="col-lg-4">
      <div className="card mb-3"><div className="card-body"><h6 className="fw-bold">Chuyển trạng thái</h6>
        {r.allowedNext.map(n => <button key={n.value} className="btn btn-outline-primary btn-sm w-100 mb-2" onClick={() => trans(n.value)}>{n.text}</button>)}
        {!r.allowedNext.length && <div className="text-muted small">Kết thúc.</div>}</div></div>
      <div className="card mb-3"><div className="card-body"><h6 className="fw-bold">Hóa đơn điện tử</h6>
        {r.eInvoice.code ? <><div className="alert alert-success py-2 small">Mã CQT: <b>{r.eInvoice.code}</b></div><a className="btn btn-sm btn-outline-primary w-100" target="_blank" href={'https://minitvan.onrender.com/Lookup/' + r.eInvoice.code}>Tra cứu T-VAN</a></>
          : <>{r.eInvoice.error && <div className="alert alert-danger py-2 small">{r.eInvoice.error}</div>}<button className="btn btn-primary btn-sm w-100" disabled={r.total <= 0} onClick={issueInv}>Xuất HĐĐT ({fmtM(r.total)})</button></>}</div></div>
      <div className="card"><div className="card-body"><h6 className="fw-bold">Quyết toán</h6>
        <button className="btn btn-success btn-sm w-100" disabled={r.total <= 0} onClick={settle}><i className="bi bi-cash-coin me-1" />Quyết toán + xuất kho</button>
        <div className="small text-muted mt-1">Ghi thanh toán + tự xuất kho phụ tùng.</div></div></div>
    </div>
    <div><Link to="/ros" className="btn btn-link">← Danh sách</Link></div>
  </div>
}

function Customers() {
  const [sp, setSp] = useSearchParams(); const q = sp.get('q') || ''
  const { data: rows, reload } = useApi('/customers' + (q ? '?q=' + encodeURIComponent(q) : ''))
  const toast = React.useContext(ToastCtx)
  const add = async () => { const name = prompt('Tên khách hàng:'); if (!name) return; const phone = prompt('SĐT:') || ''; try { await api('/customers', { method: 'POST', body: JSON.stringify({ name, phone }) }); toast('Đã thêm KH'); reload() } catch (e) { toast('❌ ' + e.message) } }
  return <div className="card"><div className="card-body">
    <div className="d-flex gap-2 mb-3"><input className="form-control form-control-sm" style={{ maxWidth: 260 }} placeholder="Tìm tên/mã…" defaultValue={q} onKeyDown={e => e.key === 'Enter' && setSp(e.target.value ? { q: e.target.value } : {})} />
      <button className="btn btn-sm btn-primary ms-auto" onClick={add}><i className="bi bi-plus" /> Thêm KH</button></div>
    <table className="table table-hover align-middle mb-0"><thead><tr><th>Mã</th><th>Tên</th><th>SĐT</th><th>Địa chỉ</th><th>MST</th><th>Đại lý</th></tr></thead>
      <tbody>{(rows || []).map(c => <tr key={c.id}><td className="small">{c.code}</td><td className="fw-semibold">{c.name}</td><td>{c.phone}</td><td className="small text-muted">{c.address}</td><td className="small">{c.taxCode}</td><td className="small">{c.dealerCode}</td></tr>)}</tbody></table>
    <div className="small text-muted mt-2">{rows?.length || 0} khách hàng (data thật từ CarService).</div>
  </div></div>
}

function Cars() {
  const { data: rows, reload } = useApi('/cars')
  const toast = React.useContext(ToastCtx)
  const add = async () => { const plate = prompt('Biển số:'); if (!plate) return; const model = prompt('Model:') || ''; const cs = await api('/customers'); if (!cs[0]) return toast('Cần có KH trước'); try { await api('/cars', { method: 'POST', body: JSON.stringify({ plate, model, customerId: cs[0].id }) }); toast('Đã thêm xe'); reload() } catch (e) { toast('❌ ' + e.message) } }
  return <div className="card"><div className="card-body"><div className="text-end mb-2"><button className="btn btn-sm btn-primary" onClick={add}><i className="bi bi-plus" /> Thêm xe</button></div>
    <table className="table table-hover align-middle mb-0"><thead><tr><th>Biển số</th><th>Model</th><th>Năm</th><th>Số khung (VIN)</th><th>Khách</th></tr></thead>
      <tbody>{(rows || []).map(c => <tr key={c.id}><td className="fw-semibold">{c.plate}</td><td>{c.model}</td><td>{c.year || ''}</td><td className="small">{c.vin}</td><td className="small">{c.customerName}</td></tr>)}</tbody></table>
  </div></div>
}

function Inventory() {
  const { data: d } = useApi('/inventory/dashboard')
  const { data: rows, reload } = useApi('/inventory/parts')
  const toast = React.useContext(ToastCtx)
  const issue = async (p) => { const q = prompt(`Xuất "${p.name}" (tồn ${p.onHand}). SL:`); if (!q) return; try { const x = await api('/inventory/issue', { method: 'POST', body: JSON.stringify({ partId: p.id, qty: +q, roId: null, reason: 'Xuất thủ công' }) }); toast(x.msg); reload() } catch (e) { toast('❌ ' + e.message) } }
  const receive = async (p) => { const q = prompt(`Nhập kho "${p.name}". SL:`); if (!q) return; try { const x = await api(`/inventory/parts/${p.id}/receive`, { method: 'POST', body: JSON.stringify({ qty: +q }) }); toast(x.msg); reload() } catch (e) { toast('❌ ' + e.message) } }
  return <>
    {d && <div className="row g-3 mb-3"><Stat t="Số phụ tùng" v={d.parts} c="#2563eb" /><Stat t="Sắp hết" v={d.lowStock} c="#dc2626" /><Stat t="Giá trị tồn" v={fmtM(d.stockValue)} c="#0f766e" sm /><Stat t="Xuất hôm nay" v={d.stockOutsToday} c="#f59e0b" /></div>}
    <div className="card"><div className="card-body">
      <table className="table table-hover align-middle mb-0"><thead><tr><th>Mã</th><th>Tên</th><th className="text-end">Giá</th><th className="text-end">Tồn</th><th className="text-end">Định mức</th><th /></tr></thead>
        <tbody>{(rows || []).map(p => <tr key={p.id} className={p.lowStock ? 'table-danger' : ''}><td className="small">{p.code}</td><td className="fw-semibold">{p.name}</td><td className="text-end">{fmtM(p.price)}</td>
          <td className={'text-end fw-bold ' + (p.lowStock ? 'text-danger' : '')}>{p.onHand} {p.unit}</td><td className="text-end text-muted">{p.minStock}</td>
          <td className="text-end"><button className="btn btn-sm btn-outline-success me-1" onClick={() => receive(p)}>Nhập</button><button className="btn btn-sm btn-outline-primary" onClick={() => issue(p)}>Xuất</button></td></tr>)}</tbody></table>
    </div></div>
  </>
}

function StockOuts() {
  const { data: rows } = useApi('/inventory/stockouts')
  return <div className="card"><div className="card-body"><table className="table table-hover align-middle mb-0"><thead><tr><th>Phiếu</th><th>Phụ tùng</th><th className="text-end">SL</th><th className="text-end">Thành tiền</th><th>RO</th><th>Lý do</th><th>Thời gian</th></tr></thead>
    <tbody>{(rows || []).map(s => <tr key={s.id}><td className="small">{s.code}</td><td>{s.partName}</td><td className="text-end">{s.quantity}</td><td className="text-end">{fmtM(s.amount)}</td><td className="small">{s.roCode}</td><td className="small text-muted">{s.reason}</td><td className="small text-muted">{new Date(s.createdAt).toLocaleString('vi-VN')}</td></tr>)}
      {rows && !rows.length && <tr><td colSpan={7} className="text-center text-muted py-3">Chưa có phiếu xuất</td></tr>}</tbody></table></div></div>
}

// ================= Layout + Router =================
const NAV = [['', 'dashboard', 'bi-speedometer2', 'Bảng điều khiển'], ['ros', 'ros', 'bi-clipboard-check', 'Lệnh sửa chữa'],
['createro', 'createro', 'bi-plus-square', 'Tiếp nhận xe'], ['customers', 'customers', 'bi-people', 'Khách hàng'],
['cars', 'cars', 'bi-car-front', 'Xe'], ['inventory', 'inventory', 'bi-box-seam', 'Tồn kho'], ['stockouts', 'stockouts', 'bi-box-arrow-up', 'Xuất kho']]

export default function App() {
  const [toast, toastNode] = useToast()
  const [apiText, setApiText] = useState('—')
  useEffect(() => { const t = setInterval(() => setApiText(lastApi.path), 400); return () => clearInterval(t) }, [])
  return <ToastCtx.Provider value={toast}>
    <div className="d-flex" style={{ background: '#f2f5fb', minHeight: '100vh', fontFamily: 'system-ui,Segoe UI,sans-serif' }}>
      <nav style={{ width: 216, minHeight: '100vh', background: '#0f1f3d', position: 'sticky', top: 0 }}>
        <div style={{ color: '#fff', fontWeight: 800, padding: 16, fontSize: 17 }}><i className="bi bi-tools" /> MiniService <span className="badge bg-primary" style={{ fontSize: 9 }}>React</span></div>
        {NAV.map(([to, , icon, label]) => <NavLink key={to} to={'/' + to} end={to === ''} style={({ isActive }) => ({ color: isActive ? '#fff' : '#9fb0cf', display: 'block', padding: '9px 16px', textDecoration: 'none', fontSize: 14, background: isActive ? 'rgba(37,99,235,.3)' : '', borderLeft: isActive ? '3px solid #2563eb' : '3px solid transparent' })}><i className={'bi ' + icon + ' me-1'} /> {label}</NavLink>)}
        <a href="/swagger" target="_blank" style={{ color: '#9fb0cf', display: 'block', padding: '9px 16px', textDecoration: 'none', fontSize: 14, borderTop: '1px solid #1c2c4d', marginTop: 8 }}><i className="bi bi-braces me-1" /> API (Swagger)</a>
      </nav>
      <div className="flex-fill">
        <div className="bg-white border-bottom px-4 py-2 d-flex justify-content-between align-items-center">
          <h6 className="mb-0 fw-bold">Quản lý dịch vụ ô tô</h6>
          <span className="small text-muted">API gọi: <code style={{ background: '#eef', padding: '1px 6px', borderRadius: 4, fontSize: 12 }}>{apiText}</code></span>
        </div>
        <div className="p-4">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/ros" element={<ROs />} />
            <Route path="/createro" element={<CreateRO />} />
            <Route path="/ro/:id" element={<RODetail />} />
            <Route path="/customers" element={<Customers />} />
            <Route path="/cars" element={<Cars />} />
            <Route path="/inventory" element={<Inventory />} />
            <Route path="/stockouts" element={<StockOuts />} />
          </Routes>
        </div>
      </div>
    </div>
    {toastNode}
  </ToastCtx.Provider>
}
