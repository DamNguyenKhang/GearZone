import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Settings {
  siteName: string;
  platformFeePercent: number;
  minPayoutAmount: number;
  maintenanceMode: boolean;
  supportEmail?: string;
}

export default function AdminSettingsPage() {
  const [settings, setSettings] = useState<Settings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    adminApi.getSettings().then(d => setSettings(d as Settings)).finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;
    setSaving(true); setError(''); setSuccess('');
    try { await adminApi.updateSettings(settings); setSuccess('Settings saved successfully!'); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed to save settings.'); }
    finally { setSaving(false); }
  };

  const set = <K extends keyof Settings>(field: K, value: Settings[K]) =>
    setSettings(s => s ? { ...s, [field]: value } : s);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!settings) return <div className="container py-4"><div className="alert alert-danger">Failed to load settings.</div></div>;

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-lg-7">
          <h2 className="mb-4">Platform Settings</h2>
          <form onSubmit={handleSubmit}>
            <div className="card shadow-sm mb-3">
              <div className="card-body">
                <div className="mb-3">
                  <label className="form-label fw-semibold">Site Name</label>
                  <input className="form-control" value={settings.siteName} onChange={e => set('siteName', e.target.value)} />
                </div>
                <div className="mb-3">
                  <label className="form-label fw-semibold">Support Email</label>
                  <input className="form-control" type="email" value={settings.supportEmail ?? ''}
                    onChange={e => set('supportEmail', e.target.value)} />
                </div>
                <div className="row g-3">
                  <div className="col-md-6">
                    <label className="form-label fw-semibold">Platform Fee (%)</label>
                    <input className="form-control" type="number" step="0.1" min="0" max="100"
                      value={settings.platformFeePercent} onChange={e => set('platformFeePercent', Number(e.target.value))} />
                  </div>
                  <div className="col-md-6">
                    <label className="form-label fw-semibold">Minimum Payout (₫)</label>
                    <input className="form-control" type="number" value={settings.minPayoutAmount}
                      onChange={e => set('minPayoutAmount', Number(e.target.value))} />
                  </div>
                </div>
                <div className="form-check mt-3">
                  <input className="form-check-input" type="checkbox" id="maintenance"
                    checked={settings.maintenanceMode} onChange={e => set('maintenanceMode', e.target.checked)} />
                  <label className="form-check-label fw-semibold" htmlFor="maintenance">Maintenance Mode</label>
                  <div className="form-text">Enabling this will show a maintenance page to all visitors.</div>
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
