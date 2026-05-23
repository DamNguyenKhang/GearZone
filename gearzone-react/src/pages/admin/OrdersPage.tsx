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

  const statusColors: Record<string, string> = {
    Pending: '#ed8936', Processing: '#3182ce', Shipping: '#805ad5',
    Delivered: '#38a169', Completed: '#38a169', Cancelled: '#e53e3e',
  };

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Orders</h1>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        <input placeholder="Search by buyer, order ID…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchOrders()}
          style={{ padding: '0.5rem', flex: '1 1 300px' }} />
        <select value={statusFilter} onChange={e => { setStatusFilter(e.target.value); fetchOrders(); }} style={{ padding: '0.5rem' }}>
          <option value="">All Statuses</option>
          <option>Pending</option><option>Processing</option><option>Shipping</option>
          <option>Delivered</option><option>Completed</option><option>Cancelled</option>
        </select>
        <button onClick={fetchOrders} style={{ padding: '0.5rem 1rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Filter</button>
      </div>

      {loading ? <div>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Order ID</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Buyer</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Total</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Payment</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Date</th>
            </tr>
          </thead>
          <tbody>
            {orders.map(o => (
              <tr key={o.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem', fontFamily: 'monospace', fontSize: 13 }}>{o.id.slice(0, 8)}</td>
                <td style={{ padding: '0.75rem' }}>{o.buyerName}</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{o.totalPrice?.toLocaleString()} VND</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ color: statusColors[o.status] ?? '#333', fontWeight: 600 }}>{o.status}</span>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center', color: '#555', fontSize: 13 }}>{o.paymentStatus}</td>
                <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(o.createdAt).toLocaleDateString()}</td>
              </tr>
            ))}
            {orders.length === 0 && (
              <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No orders found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
