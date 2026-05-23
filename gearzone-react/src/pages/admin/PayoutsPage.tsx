import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Payout {
  id: string;
  storeName: string;
  amount: number;
  status: string;
  requestedAt: string;
  processedAt?: string;
  bankName?: string;
  bankAccount?: string;
}

const statusBadge: Record<string, string> = { Completed: 'success', Pending: 'warning', Rejected: 'danger' };

export default function AdminPayoutsPage() {
  const [payouts, setPayouts] = useState<Payout[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchPayouts = () => {
    setLoading(true);
    adminApi.payouts.list()
      .then(d => setPayouts((d as { items?: Payout[] }).items ?? (d as Payout[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchPayouts(); }, []);

  const handleProcess = async (id: string) => {
    setProcessing(id);
    try { await adminApi.payouts.process(id); fetchPayouts(); }
    finally { setProcessing(null); }
  };

  const handleReject = async (id: string) => {
    setProcessing(id);
    try { await adminApi.payouts.reject(id, 'Rejected by admin'); fetchPayouts(); }
    finally { setProcessing(null); }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <h2 className="mb-4">Payout Requests</h2>
      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr><th>Store</th><th className="text-end">Amount</th><th>Bank Info</th><th className="text-center">Status</th><th>Requested</th><th className="text-center">Actions</th></tr>
            </thead>
            <tbody>
              {payouts.map(p => (
                <tr key={p.id}>
                  <td className="fw-semibold">{p.storeName}</td>
                  <td className="text-end fw-bold">{p.amount?.toLocaleString()} ₫</td>
                  <td>
                    {p.bankName && <div className="small">{p.bankName}</div>}
                    {p.bankAccount && <small className="text-muted">{p.bankAccount}</small>}
                  </td>
                  <td className="text-center">
                    <span className={`badge bg-${statusBadge[p.status] ?? 'secondary'}`}>{p.status}</span>
                  </td>
                  <td className="text-muted small">{new Date(p.requestedAt).toLocaleDateString()}</td>
                  <td className="text-center">
                    {p.status === 'Pending' && (
                      <div className="d-flex gap-1 justify-content-center">
                        <button className="btn btn-sm btn-outline-success" disabled={processing === p.id}
                          onClick={() => handleProcess(p.id)}>
                          {processing === p.id ? <span className="spinner-border spinner-border-sm" /> : 'Process'}
                        </button>
                        <button className="btn btn-sm btn-outline-danger" disabled={processing === p.id}
                          onClick={() => handleReject(p.id)}>Reject</button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
              {payouts.length === 0 && <tr><td colSpan={6} className="text-center text-muted py-4">No payout requests.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
