import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { sellerApi } from '../api/seller';

interface RegistrationProgress {
  currentStep: number;
  status: string;
  rejectionReason?: string;
  step1Data?: Record<string, unknown>;
  step2Data?: Record<string, unknown>;
  step3Data?: Record<string, unknown>;
}

export default function RegisterSellerPage() {
  const navigate = useNavigate();
  const [progress, setProgress] = useState<RegistrationProgress | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const [storeName, setStoreName] = useState('');
  const [storeDesc, setStoreDesc] = useState('');
  const [businessName, setBusinessName] = useState('');
  const [taxId, setTaxId] = useState('');
  const [bankName, setBankName] = useState('');
  const [bankAccount, setBankAccount] = useState('');
  const [bankHolder, setBankHolder] = useState('');

  const refresh = async () => {
    const p = await sellerApi.registration.getProgress();
    setProgress(p as RegistrationProgress);
  };

  useEffect(() => {
    sellerApi.registration.getProgress()
      .then(d => setProgress(d as RegistrationProgress))
      .finally(() => setLoading(false));
  }, []);

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const wrap = (fn: () => Promise<any>) => async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try { await fn(); await refresh(); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  if (progress?.status === 'Approved') {
    return (
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-md-5 text-center py-5">
            <div className="text-success display-1 mb-3">✓</div>
            <h3 className="text-success">Application Approved!</h3>
            <p className="text-muted mb-4">Your seller account has been approved.</p>
            <button className="btn btn-success btn-lg" onClick={() => navigate('/seller/dashboard')}>
              Go to Seller Dashboard
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (progress?.status === 'PendingReview') {
    return (
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-md-5 text-center py-5">
            <div className="spinner-border text-primary mb-3" />
            <h3>Application Under Review</h3>
            <p className="text-muted">Your application is being reviewed. We'll notify you via email.</p>
          </div>
        </div>
      </div>
    );
  }

  if (progress?.status === 'Rejected') {
    return (
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-md-5 text-center py-5">
            <h3 className="text-danger">Application Rejected</h3>
            {progress.rejectionReason && (
              <div className="alert alert-danger">{progress.rejectionReason}</div>
            )}
            {error && <div className="alert alert-danger">{error}</div>}
            <button className="btn btn-primary" onClick={async () => {
              setSaving(true);
              try { await sellerApi.registration.reapply(); await refresh(); }
              catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
              finally { setSaving(false); }
            }} disabled={saving}>
              {saving ? <span className="spinner-border spinner-border-sm me-2" /> : null}Reapply
            </button>
          </div>
        </div>
      </div>
    );
  }

  const step = progress?.currentStep ?? 1;
  const steps = ['Store Info', 'Business', 'Banking'];

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <h2 className="mb-4">Become a Seller</h2>

          <div className="d-flex mb-4 gap-2">
            {steps.map((label, i) => {
              const n = i + 1;
              const done = n < step;
              const active = n === step;
              return (
                <div key={n} className={`flex-fill text-center py-2 rounded fw-semibold small
                  ${done ? 'bg-success text-white' : active ? 'bg-primary text-white' : 'bg-light text-muted'}`}>
                  {done ? '✓ ' : `${n}. `}{label}
                </div>
              );
            })}
          </div>

          {error && <div className="alert alert-danger">{error}</div>}

          {step === 1 && (
            <form onSubmit={wrap(() => sellerApi.registration.submitStep1({ storeName, description: storeDesc }))}>
              <div className="mb-3">
                <label className="form-label">Store Name</label>
                <input className="form-control" value={storeName} onChange={e => setStoreName(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">Store Description</label>
                <textarea className="form-control" rows={3} value={storeDesc} onChange={e => setStoreDesc(e.target.value)} />
              </div>
              <button type="submit" className="btn btn-primary w-100" disabled={saving}>
                {saving ? <span className="spinner-border spinner-border-sm me-2" /> : null}Continue to Step 2
              </button>
            </form>
          )}

          {step === 2 && (
            <form onSubmit={wrap(() => sellerApi.registration.submitStep2({ businessName, taxId }))}>
              <div className="mb-3">
                <label className="form-label">Business / Company Name</label>
                <input className="form-control" value={businessName} onChange={e => setBusinessName(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">Tax ID (optional)</label>
                <input className="form-control" value={taxId} onChange={e => setTaxId(e.target.value)} />
              </div>
              <button type="submit" className="btn btn-primary w-100" disabled={saving}>
                {saving ? <span className="spinner-border spinner-border-sm me-2" /> : null}Continue to Step 3
              </button>
            </form>
          )}

          {step === 3 && (
            <form onSubmit={wrap(() => sellerApi.registration.submitStep3({ bankName, bankAccount, bankHolder }))}>
              <div className="mb-3">
                <label className="form-label">Bank Name</label>
                <input className="form-control" value={bankName} onChange={e => setBankName(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">Account Number</label>
                <input className="form-control" value={bankAccount} onChange={e => setBankAccount(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">Account Holder Name</label>
                <input className="form-control" value={bankHolder} onChange={e => setBankHolder(e.target.value)} required />
              </div>
              <button type="submit" className="btn btn-primary w-100" disabled={saving}>
                {saving ? <span className="spinner-border spinner-border-sm me-2" /> : null}Save Banking Info
              </button>
            </form>
          )}

          {step === 4 && (
            <div className="text-center">
              <p className="text-success fw-semibold">All steps completed! Ready to submit.</p>
              <button className="btn btn-success btn-lg" disabled={saving} onClick={async () => {
                setSaving(true);
                try { await sellerApi.registration.submit(); await refresh(); }
                catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
                finally { setSaving(false); }
              }}>
                {saving ? <span className="spinner-border spinner-border-sm me-2" /> : null}Submit Application
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
