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

  // Step 1: Store info
  const [storeName, setStoreName] = useState('');
  const [storeDesc, setStoreDesc] = useState('');
  // Step 2: Business info
  const [businessName, setBusinessName] = useState('');
  const [taxId, setTaxId] = useState('');
  // Step 3: Banking info
  const [bankName, setBankName] = useState('');
  const [bankAccount, setBankAccount] = useState('');
  const [bankHolder, setBankHolder] = useState('');

  useEffect(() => {
    sellerApi.registration.getProgress()
      .then(d => setProgress(d as RegistrationProgress))
      .finally(() => setLoading(false));
  }, []);

  const handleStep1 = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await sellerApi.registration.submitStep1({ storeName, description: storeDesc });
      const p = await sellerApi.registration.getProgress();
      setProgress(p as RegistrationProgress);
    } catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleStep2 = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await sellerApi.registration.submitStep2({ businessName, taxId });
      const p = await sellerApi.registration.getProgress();
      setProgress(p as RegistrationProgress);
    } catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleStep3 = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await sellerApi.registration.submitStep3({ bankName, bankAccount, bankHolder });
      const p = await sellerApi.registration.getProgress();
      setProgress(p as RegistrationProgress);
    } catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleSubmit = async () => {
    setSaving(true); setError('');
    try {
      await sellerApi.registration.submit();
      const p = await sellerApi.registration.getProgress();
      setProgress(p as RegistrationProgress);
    } catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleReapply = async () => {
    setSaving(true); setError('');
    try {
      await sellerApi.registration.reapply();
      const p = await sellerApi.registration.getProgress();
      setProgress(p as RegistrationProgress);
    } catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  if (progress?.status === 'Approved') {
    return (
      <div style={{ padding: '2rem', maxWidth: 500, margin: '2rem auto', textAlign: 'center' }}>
        <h1 style={{ color: '#38a169' }}>Application Approved!</h1>
        <p>Your seller account has been approved.</p>
        <button onClick={() => navigate('/seller/dashboard')}
          style={{ padding: '0.75rem 2rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          Go to Seller Dashboard
        </button>
      </div>
    );
  }

  if (progress?.status === 'PendingReview') {
    return (
      <div style={{ padding: '2rem', maxWidth: 500, margin: '2rem auto', textAlign: 'center' }}>
        <h1>Application Under Review</h1>
        <p style={{ color: '#555' }}>Your application is being reviewed. We'll notify you via email.</p>
      </div>
    );
  }

  if (progress?.status === 'Rejected') {
    return (
      <div style={{ padding: '2rem', maxWidth: 500, margin: '2rem auto', textAlign: 'center' }}>
        <h1 style={{ color: '#e53e3e' }}>Application Rejected</h1>
        {progress.rejectionReason && <p style={{ color: '#555' }}><strong>Reason:</strong> {progress.rejectionReason}</p>}
        {error && <p style={{ color: 'red' }}>{error}</p>}
        <button onClick={handleReapply} disabled={saving}
          style={{ padding: '0.75rem 2rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          {saving ? 'Processing…' : 'Reapply'}
        </button>
      </div>
    );
  }

  const step = progress?.currentStep ?? 1;

  return (
    <div style={{ padding: '2rem', maxWidth: 500, margin: '2rem auto' }}>
      <h1>Become a Seller</h1>

      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '2rem' }}>
        {[1, 2, 3].map(n => (
          <div key={n} style={{
            flex: 1, padding: '0.5rem', textAlign: 'center', borderRadius: 4,
            background: n < step ? '#38a169' : n === step ? '#3182ce' : '#e2e8f0',
            color: n <= step ? '#fff' : '#888', fontWeight: 600, fontSize: 14
          }}>
            {n < step ? '✓' : n}. {n === 1 ? 'Store Info' : n === 2 ? 'Business' : 'Banking'}
          </div>
        ))}
      </div>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {step === 1 && (
        <form onSubmit={handleStep1} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <input placeholder="Store Name" value={storeName} onChange={e => setStoreName(e.target.value)} required style={{ padding: '0.5rem' }} />
          <textarea placeholder="Store Description" value={storeDesc} onChange={e => setStoreDesc(e.target.value)} rows={3} style={{ padding: '0.5rem' }} />
          <button type="submit" disabled={saving} style={{ padding: '0.75rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
            {saving ? 'Saving…' : 'Continue to Step 2'}
          </button>
        </form>
      )}

      {step === 2 && (
        <form onSubmit={handleStep2} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <input placeholder="Business/Company Name" value={businessName} onChange={e => setBusinessName(e.target.value)} required style={{ padding: '0.5rem' }} />
          <input placeholder="Tax ID (optional)" value={taxId} onChange={e => setTaxId(e.target.value)} style={{ padding: '0.5rem' }} />
          <button type="submit" disabled={saving} style={{ padding: '0.75rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
            {saving ? 'Saving…' : 'Continue to Step 3'}
          </button>
        </form>
      )}

      {step === 3 && (
        <form onSubmit={handleStep3} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <input placeholder="Bank Name" value={bankName} onChange={e => setBankName(e.target.value)} required style={{ padding: '0.5rem' }} />
          <input placeholder="Account Number" value={bankAccount} onChange={e => setBankAccount(e.target.value)} required style={{ padding: '0.5rem' }} />
          <input placeholder="Account Holder Name" value={bankHolder} onChange={e => setBankHolder(e.target.value)} required style={{ padding: '0.5rem' }} />
          <button type="submit" disabled={saving} style={{ padding: '0.75rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
            {saving ? 'Saving…' : 'Save Banking Info'}
          </button>
        </form>
      )}

      {step === 4 && (
        <div style={{ textAlign: 'center' }}>
          <p style={{ color: '#38a169' }}>All steps completed! Ready to submit.</p>
          <button onClick={handleSubmit} disabled={saving}
            style={{ padding: '0.75rem 2rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            {saving ? 'Submitting…' : 'Submit Application'}
          </button>
        </div>
      )}
    </div>
  );
}
