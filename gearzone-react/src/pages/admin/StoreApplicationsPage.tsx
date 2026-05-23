import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Application {
  id: string;
  applicantName: string;
  applicantEmail: string;
  storeName: string;
  businessName?: string;
  status: string;
  submittedAt: string;
}

export default function AdminStoreApplicationsPage() {
  const [applications, setApplications] = useState<Application[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<string | null>(null);
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const fetchApps = () => {
    setLoading(true);
    adminApi.storeApplications.list()
      .then(d => setApplications((d as { items?: Application[] }).items ?? (d as Application[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchApps(); }, []);

  const handleApprove = async (id: string) => {
    setProcessing(id);
    try { await adminApi.storeApplications.approve(id); fetchApps(); }
    finally { setProcessing(null); }
  };

  const handleReject = async () => {
    if (!rejectId) return;
    setProcessing(rejectId);
    try {
      await adminApi.storeApplications.reject(rejectId, rejectReason);
      setRejectId(null); setRejectReason('');
      fetchApps();
    } finally { setProcessing(null); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Store Applications</h1>

      {rejectId && (
        <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 400, width: '90%' }}>
            <h3>Rejection Reason</h3>
            <textarea value={rejectReason} onChange={e => setRejectReason(e.target.value)} placeholder="Enter reason for rejection…" rows={4}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem' }} />
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleReject} disabled={!rejectReason || !!processing}
                style={{ flex: 1, padding: '0.5rem', background: '#e53e3e', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Confirm Reject
              </button>
              <button onClick={() => { setRejectId(null); setRejectReason(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Applicant</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store Name</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Submitted</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {applications.map(a => (
            <tr key={a.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem' }}>
                <div style={{ fontWeight: 500 }}>{a.applicantName}</div>
                <div style={{ fontSize: 12, color: '#888' }}>{a.applicantEmail}</div>
              </td>
              <td style={{ padding: '0.75rem' }}>
                <div>{a.storeName}</div>
                {a.businessName && <div style={{ fontSize: 12, color: '#888' }}>{a.businessName}</div>}
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: a.status === 'PendingReview' ? '#ed8936' : a.status === 'Approved' ? '#38a169' : '#e53e3e', fontWeight: 600 }}>
                  {a.status === 'PendingReview' ? 'Pending' : a.status}
                </span>
              </td>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(a.submittedAt).toLocaleDateString()}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                {a.status === 'PendingReview' && (
                  <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center' }}>
                    <button onClick={() => handleApprove(a.id)} disabled={processing === a.id}
                      style={{ padding: '0.25rem 0.75rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749' }}>
                      {processing === a.id ? '…' : 'Approve'}
                    </button>
                    <button onClick={() => setRejectId(a.id)}
                      style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>
                      Reject
                    </button>
                  </div>
                )}
              </td>
            </tr>
          ))}
          {applications.length === 0 && (
            <tr><td colSpan={5} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No applications found.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
