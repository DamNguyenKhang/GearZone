import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Order {
  id: string;
  buyerName: string;
  totalPrice: number;
  status: string;
  paymentStatus: string;
  createdAt: string;
  subOrderCount: number;
}

const statusBadge: Record<string, string> = {
  Pending: 'warning', Processing: 'primary', Shipping: 'info',
  Delivered: 'success', Completed: 'success', Cancelled: 'danger',
};

export default function AdminOrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const fetchOrders = () => {
    setLoading(true);
    adminApi.orders.list({ search, status: statusFilter || undefined })
      .then(d => setOrders((d as { items?: Order[] }).items ?? (d as Order[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchOrders(); }, []);

  return (
    <div className="container">
      <h2 className="mb-4">Orders</h2>

      <div className="d-flex gap-2 mb-4 flex-wrap">
        <input className="form-control" style={{ maxWidth: 300 }}
          placeholder="Search by buyer, order ID…" value={search}
          onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchOrders()} />
        <select className="form-select" style={{ maxWidth: 200 }}
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); }}>
          <option value="">All Statuses</option>
          {['Pending', 'Processing', 'Shipping', 'Delivered', 'Completed', 'Cancelled'].map(s => (
            <option key={s}>{s}</option>
          ))}
        </select>
        <button className="btn btn-primary" onClick={fetchOrders}>Filter</button>
      </div>

      {loading ? (
        <div className="text-center py-4"><div className="spinner-border text-primary" /></div>
      ) : (
        <div className="card shadow-sm">
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr>
                  <th>Order ID</th><th>Buyer</th><th className="text-end">Total</th>
                  <th className="text-center">Status</th>
                  <th className="text-center">Payment</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {orders.map(o => (
                  <tr key={o.id}>
                    <td className="font-monospace small">{o.id.slice(0, 8)}</td>
                    <td>{o.buyerName}</td>
                    <td className="text-end fw-semibold">{o.totalPrice?.toLocaleString()} ₫</td>
                    <td className="text-center">
                      <span className={`badge bg-${statusBadge[o.status] ?? 'secondary'}`}>{o.status}</span>
                    </td>
                    <td className="text-center text-muted small">{o.paymentStatus}</td>
                    <td className="text-muted small">{new Date(o.createdAt).toLocaleDateString()}</td>
                  </tr>
                ))}
                {orders.length === 0 && (
                  <tr><td colSpan={6} className="text-center text-muted py-4">No orders found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
