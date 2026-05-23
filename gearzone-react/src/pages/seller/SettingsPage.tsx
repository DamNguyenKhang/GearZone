import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface StoreSettings {
  name: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  contactEmail?: string;
  contactPhone?: string;
  returnPolicy?: string;
  shippingPolicy?: string;
}

export default function SellerSettingsPage() {
  const [settings, setSettings] = useState<StoreSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    sellerApi.getStoreSettings().then(d => setSettings(d as StoreSettings)).finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;
    setSaving(true); setError(''); setSuccess('');
    try {
      await sellerApi.updateStoreSettings(settings);
      setSuccess('Settings saved successfully!');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save settings.');
    } finally { setSaving(false); }
  };

  const set = (field: keyof StoreSettings, value: string) =>
    setSettings(s => s ? { ...s, [field]: value } : s);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!settings) return <div className="container py-4"><div className="alert alert-danger">Failed to load settings.</div></div>;

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-lg-7">
          <h2 className="mb-4">Store Settings</h2>
          <form onSubmit={handleSubmit}>
            <div className="card shadow-sm mb-3">
              <div className="card-header">Basic Info</div>
              <div className="card-body">
                <div className="mb-3">
                  <label className="form-label fw-semibold">Store Name</label>
                  <input className="form-control" value={settings.name} onChange={e => set('name', e.target.value)} required />
                </div>
                <div className="mb-3">
                  <label className="form-label fw-semibold">Description</label>
                  <textarea className="form-control" rows={3} value={settings.description ?? ''} onChange={e => set('description', e.target.value)} />
                </div>
                <div className="row g-3">
                  <div className="col-md-6">
                    <label className="form-label fw-semibold">Contact Email</label>
                    <input className="form-control" type="email" value={settings.contactEmail ?? ''} onChange={e => set('contactEmail', e.target.value)} />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label fw-semibold">Contact Phone</label>
                    <input className="form-control" value={settings.contactPhone ?? ''} onChange={e => set('contactPhone', e.target.value)} />
                  </div>
                </div>
              </div>
            </div>

            <div className="card shadow-sm mb-3">
              <div className="card-header">Policies</div>
              <div className="card-body">
                <div className="mb-3">
                  <label className="form-label fw-semibold">Return Policy</label>
                  <textarea className="form-control" rows={4} value={settings.returnPolicy ?? ''} onChange={e => set('returnPolicy', e.target.value)} />
                </div>
                <div className="mb-0">
                  <label className="form-label fw-semibold">Shipping Policy</label>
                  <textarea className="form-control" rows={4} value={settings.shippingPolicy ?? ''} onChange={e => set('shippingPolicy', e.target.value)} />
                </div>
              </div>
            </div>

            {error && <div className="alert alert-danger">{error}</div>}
            {success && <div className="alert alert-success">{success}</div>}

            <button type="submit" className="btn btn-primary w-100" disabled={saving}>
              {saving ? <span className="spinner-border spinner-border-sm me-2" /> : null}Save Settings
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
