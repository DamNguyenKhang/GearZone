import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface AdminStats {
  totalUsers: number;
  totalOrders: number;
  totalRevenue: number;
  totalStores: number;
  pendingStoreApplications: number;
  pendingPayouts: number;
}

export default function AdminDashboardPage() {
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi.getDashboard().then(d => setStats(d as AdminStats)).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!stats) return <div className="container py-4"><div className="alert alert-danger">Failed to load dashboard.</div></div>;

  const statCards = [
    { label: 'Total Users', value: stats.totalUsers, color: 'primary', link: '/admin/users' },
    { label: 'Total Orders', value: stats.totalOrders, color: 'warning', link: '/admin/orders' },
    { label: 'Total Revenue', value: `${(stats.totalRevenue ?? 0).toLocaleString()} ₫`, color: 'success', link: '/admin/transactions' },
    { label: 'Active Stores', value: stats.totalStores, color: 'info', link: '/admin/stores' },
    { label: 'Pending Applications', value: stats.pendingStoreApplications, color: 'danger', link: '/admin/store-applications' },
    { label: 'Pending Payouts', value: stats.pendingPayouts, color: 'secondary', link: '/admin/payouts' },
  ];

  const quickLinks = [
    { to: '/admin/products', label: 'Products' },
    { to: '/admin/brands', label: 'Brands' },
    { to: '/admin/categories', label: 'Categories' },
    { to: '/admin/vouchers', label: 'Vouchers' },
    { to: '/admin/settings', label: 'Settings' },
    { to: '/admin/wallet', label: 'Wallet' },
  ];

  return (
    <div className="container">
      <h2 className="mb-4">Admin Dashboard</h2>

      <div className="row g-3 mb-4">
        {statCards.map(s => (
          <div key={s.label} className="col-sm-6 col-lg-4">
            <Link to={s.link} className="text-decoration-none">
              <div className={`card shadow-sm border-start border-4 border-${s.color} h-100`}>
                <div className="card-body">
                  <p className="text-muted small mb-1">{s.label}</p>
                  <p className={`h4 fw-bold text-${s.color} mb-0`}>{s.value}</p>
                </div>
              </div>
            </Link>
          </div>
        ))}
      </div>

      <h5 className="mb-3">Quick Access</h5>
      <div className="d-flex flex-wrap gap-2">
        {quickLinks.map(l => (
          <Link key={l.to} to={l.to} className="btn btn-outline-secondary btn-sm">{l.label}</Link>
        ))}
      </div>
    </div>
  );
}
