import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface WalletData {
  platformBalance: number;
  totalHeld: number;
  totalPaidOut: number;
  recentTransactions?: Array<{ id: string; type: string; amount: number; createdAt: string; description?: string }>;
}

export default function AdminWalletPage() {
  const [data, setData] = useState<WalletData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi.getWallet().then(d => setData(d as WalletData)).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!data) return <div className="container py-4"><div className="alert alert-danger">Failed to load wallet data.</div></div>;

  const stats = [
    { label: 'Platform Balance', value: data.platformBalance, color: 'success' },
    { label: 'Held for Payouts', value: data.totalHeld, color: 'warning' },
    { label: 'Total Paid Out', value: data.totalPaidOut, color: 'primary' },
  ];

  return (
    <div className="container">
      <h2 className="mb-4">Platform Wallet</h2>

      <div className="row g-3 mb-4">
        {stats.map(s => (
          <div key={s.label} className="col-md-4">
            <div className={`card shadow-sm border-start border-4 border-${s.color}`}>
              <div className="card-body">
                <p className="text-muted small mb-1">{s.label}</p>
                <p className={`h4 fw-bold text-${s.color} mb-0`}>{(s.value ?? 0).toLocaleString()} ₫</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="mb-4">
        <Link to="/admin/transactions" className="btn btn-primary">View All Transactions</Link>
      </div>

      {data.recentTransactions && data.recentTransactions.length > 0 && (
        <div className="card shadow-sm">
          <div className="card-header"><h5 className="mb-0">Recent Transactions</h5></div>
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr><th>Date</th><th>Type</th><th className="text-end">Amount</th><th>Description</th></tr>
              </thead>
              <tbody>
                {data.recentTransactions.map(t => (
                  <tr key={t.id}>
                    <td className="text-muted small">{new Date(t.createdAt).toLocaleDateString()}</td>
                    <td>{t.type}</td>
                    <td className="text-end fw-semibold">{t.amount?.toLocaleString()} ₫</td>
                    <td className="text-muted small">{t.description ?? '—'}</td>
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
