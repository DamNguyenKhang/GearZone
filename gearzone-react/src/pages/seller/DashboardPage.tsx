import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { sellerApi } from '../../api/seller';

interface DashboardData {
  totalRevenue: number;
  totalOrders: number;
  pendingOrders: number;
  totalProducts: number;
  recentOrders?: Array<{ id: string; status: string; totalPrice: number; createdAt: string }>;
}

const statusBadge: Record<string, string> = {
  Pending: 'warning', Approved: 'primary', Processing: 'info',
  Delivered: 'success', Cancelled: 'danger',
};

export default function SellerDashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    sellerApi.getDashboard().then(d => setData(d as DashboardData)).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!data) return <div className="container py-4"><div className="alert alert-danger">Failed to load dashboard.</div></div>;

  const stats = [
    { label: 'Total Revenue', value: `${(data.totalRevenue ?? 0).toLocaleString()} ₫`, color: 'success' },
    { label: 'Total Orders', value: data.totalOrders ?? 0, color: 'primary' },
    { label: 'Pending Orders', value: data.pendingOrders ?? 0, color: 'warning' },
    { label: 'Products Listed', value: data.totalProducts ?? 0, color: 'info' },
  ];

  return (
    <div className="container">
      <h2 className="mb-4">Seller Dashboard</h2>

      <div className="row g-3 mb-4">
        {stats.map(s => (
          <div key={s.label} className="col-sm-6 col-lg-3">
            <div className={`card shadow-sm border-start border-4 border-${s.color}`}>
              <div className="card-body">
                <p className="text-muted small mb-1">{s.label}</p>
                <p className={`h4 fw-bold text-${s.color} mb-0`}>{s.value}</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="d-flex gap-2 mb-4 flex-wrap">
        <Link to="/seller/products" className="btn btn-primary">Manage Products</Link>
        <Link to="/seller/orders" className="btn btn-success">Manage Orders</Link>
        <Link to="/seller/revenue" className="btn btn-info text-white">View Revenue</Link>
        <Link to="/seller/vouchers" className="btn btn-outline-secondary">Vouchers</Link>
        <Link to="/seller/settings" className="btn btn-outline-secondary">Settings</Link>
      </div>

      {data.recentOrders && data.recentOrders.length > 0 && (
        <div className="card shadow-sm">
          <div className="card-header"><h5 className="mb-0">Recent Orders</h5></div>
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr><th>Order ID</th><th>Status</th><th className="text-end">Amount</th><th>Date</th></tr>
              </thead>
              <tbody>
                {data.recentOrders.map(o => (
                  <tr key={o.id}>
                    <td className="font-monospace">{o.id.slice(0, 8)}</td>
                    <td><span className={`badge bg-${statusBadge[o.status] ?? 'secondary'}`}>{o.status}</span></td>
                    <td className="text-end fw-semibold">{o.totalPrice?.toLocaleString()} ₫</td>
                    <td className="text-muted small">{new Date(o.createdAt).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
