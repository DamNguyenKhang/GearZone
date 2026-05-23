import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface RevenueData {
  totalRevenue: number;
  pendingPayout: number;
  completedPayout: number;
  transactions: Array<{ id: string; amount: number; type: string; status: string; createdAt: string; note?: string }>;
}

const txStatusBadge: Record<string, string> = { Completed: 'success', Pending: 'warning', Failed: 'danger' };

export default function SellerRevenuePage() {
  const [data, setData] = useState<RevenueData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    sellerApi.getRevenue().then(d => setData(d as RevenueData)).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!data) return <div className="container py-4"><div className="alert alert-danger">Failed to load revenue data.</div></div>;

  const stats = [
    { label: 'Total Revenue', value: data.totalRevenue, color: 'success' },
    { label: 'Pending Payout', value: data.pendingPayout, color: 'warning' },
    { label: 'Completed Payout', value: data.completedPayout, color: 'primary' },
  ];

  return (
    <div className="container">
      <h2 className="mb-4">Revenue</h2>

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

      <div className="card shadow-sm">
        <div className="card-header"><h5 className="mb-0">Payout Transactions</h5></div>
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr><th>Date</th><th>Type</th><th className="text-end">Amount</th><th>Status</th><th>Note</th></tr>
            </thead>
            <tbody>
              {data.transactions?.map(t => (
                <tr key={t.id}>
                  <td className="text-muted small">{new Date(t.createdAt).toLocaleDateString()}</td>
                  <td>{t.type}</td>
                  <td className="text-end fw-semibold">{t.amount?.toLocaleString()} ₫</td>
                  <td><span className={`badge bg-${txStatusBadge[t.status] ?? 'secondary'}`}>{t.status}</span></td>
                  <td className="text-muted small">{t.note ?? '—'}</td>
                </tr>
              ))}
              {(!data.transactions || data.transactions.length === 0) && (
                <tr><td colSpan={5} className="text-center text-muted py-4">No transactions yet.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
