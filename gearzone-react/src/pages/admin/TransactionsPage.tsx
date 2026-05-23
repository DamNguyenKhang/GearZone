import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Transaction {
  id: string;
  type: string;
  amount: number;
  status: string;
  storeName?: string;
  orderId?: string;
  createdAt: string;
  description?: string;
}

const statusBadge: Record<string, string> = { Completed: 'success', Pending: 'warning', Failed: 'danger' };

export default function AdminTransactionsPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [typeFilter, setTypeFilter] = useState('');

  const fetchTransactions = () => {
    setLoading(true);
    adminApi.getTransactions({ type: typeFilter || undefined })
      .then(d => setTransactions((d as { items?: Transaction[] }).items ?? (d as Transaction[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchTransactions(); }, []);

  return (
    <div className="container">
      <h2 className="mb-4">Transactions</h2>

      <div className="d-flex gap-2 mb-4">
        <select className="form-select" style={{ maxWidth: 200 }}
          value={typeFilter} onChange={e => setTypeFilter(e.target.value)}>
          <option value="">All Types</option>
          {['Payment', 'Payout', 'Refund', 'PlatformFee'].map(t => <option key={t}>{t}</option>)}
        </select>
        <button className="btn btn-primary" onClick={fetchTransactions}>Filter</button>
      </div>

      {loading ? (
        <div className="text-center py-4"><div className="spinner-border text-primary" /></div>
      ) : (
        <div className="card shadow-sm">
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr><th>Date</th><th>Type</th><th className="text-end">Amount</th><th>Store</th><th className="text-center">Status</th><th>Description</th></tr>
              </thead>
              <tbody>
                {transactions.map(t => (
                  <tr key={t.id}>
                    <td className="text-muted small">{new Date(t.createdAt).toLocaleDateString()}</td>
                    <td>{t.type}</td>
                    <td className="text-end fw-semibold">{t.amount?.toLocaleString()} ₫</td>
                    <td className="text-muted small">{t.storeName ?? '—'}</td>
                    <td className="text-center">
                      <span className={`badge bg-${statusBadge[t.status] ?? 'secondary'}`}>{t.status}</span>
                    </td>
                    <td className="text-muted small">{t.description ?? '—'}</td>
                  </tr>
                ))}
                {transactions.length === 0 && <tr><td colSpan={6} className="text-center text-muted py-4">No transactions found.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
