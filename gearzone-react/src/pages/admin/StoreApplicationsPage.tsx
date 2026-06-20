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

const statusBadge: Record<string, string> = { PendingReview: 'warning', Approved: 'success', Rejected: 'danger' };

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
      setRejectId(null); setRejectReason(''); fetchApps();
    } finally { setProcessing(null); }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <h2 className="mb-4">Store Applications</h2>

      {rejectId && (
        <div className="modal d-block" style={{ background: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Rejection Reason</h5>
                <button className="btn-close" onClick={() => { setRejectId(null); setRejectReason(''); }} />
              </div>
              <div className="modal-body">
                <textarea className="form-control" rows={4} value={rejectReason}
                  onChange={e => setRejectReason(e.target.value)}
                  placeholder="Enter reason for rejection…" />
              </div>
              <div className="modal-footer">
                <button className="btn btn-outline-secondary" onClick={() => { setRejectId(null); setRejectReason(''); }}>Cancel</button>
                <button className="btn btn-danger" disabled={!rejectReason || !!processing} onClick={handleReject}>
                  {processing ? <span className="spinner-border spinner-border-sm me-1" /> : null}Confirm Reject
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr><th>Applicant</th><th>Store</th><th className="text-center">Status</th><th>Submitted</th><th className="text-center">Actions</th></tr>
            </thead>
            <tbody>
              {applications.map(a => (
                <tr key={a.id}>
                  <td>
                    <div className="fw-semibold">{a.applicantName}</div>
                    <small className="text-muted">{a.applicantEmail}</small>
                  </td>
                  <td>
                    <div>{a.storeName}</div>
                    {a.businessName && <small className="text-muted">{a.businessName}</small>}
                  </td>
                  <td className="text-center">
                    <span className={`badge bg-${statusBadge[a.status] ?? 'secondary'}`}>
                      {a.status === 'PendingReview' ? 'Pending' : a.status}
                    </span>
                  </td>
                  <td className="text-muted small">{new Date(a.submittedAt).toLocaleDateString()}</td>
                  <td className="text-center">
                    {a.status === 'PendingReview' && (
                      <div className="d-flex gap-1 justify-content-center">
                        <button className="btn btn-sm btn-outline-success" disabled={processing === a.id}
                          onClick={() => handleApprove(a.id)}>
                          {processing === a.id ? <span className="spinner-border spinner-border-sm" /> : 'Approve'}
                        </button>
                        <button className="btn btn-sm btn-outline-danger" onClick={() => setRejectId(a.id)}>Reject</button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
              {applications.length === 0 && <tr><td colSpan={5} className="text-center text-muted py-4">No applications found.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
