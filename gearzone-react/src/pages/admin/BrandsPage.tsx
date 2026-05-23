import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Brand {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  productCount?: number;
}

export default function AdminBrandsPage() {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', slug: '', logoUrl: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchBrands = () => {
    adminApi.brands.list()
      .then(d => setBrands((d as { items?: Brand[] }).items ?? (d as Brand[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchBrands(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      if (editId) {
        await adminApi.brands.update(editId, form);
      } else {
        await adminApi.brands.create(form);
      }
      setShowForm(false); setEditId(null); setForm({ name: '', slug: '', logoUrl: '' });
      fetchBrands();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save brand.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this brand?')) return;
    await adminApi.brands.delete(id);
    fetchBrands();
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Brands</h1>
        <button onClick={() => { setShowForm(true); setEditId(null); setForm({ name: '', slug: '', logoUrl: '' }); }}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          + Add Brand
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem', maxWidth: 400 }}>
          <h3 style={{ margin: 0 }}>{editId ? 'Edit Brand' : 'New Brand'}</h3>
          <input placeholder="Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <input placeholder="Slug (e.g. asus)" value={form.slug} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <input placeholder="Logo URL (optional)" value={form.logoUrl} onChange={e => setForm(f => ({ ...f, logoUrl: e.target.value }))} style={{ padding: '0.5rem' }} />
          {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </form>
      )}

      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem' }}>
        {brands.map(b => (
          <div key={b.id} style={{ width: 200, border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', textAlign: 'center' }}>
            {b.logoUrl && <img src={b.logoUrl} alt={b.name} style={{ width: 60, height: 60, objectFit: 'contain', marginBottom: '0.5rem' }} />}
            <p style={{ margin: 0, fontWeight: 600 }}>{b.name}</p>
            <p style={{ margin: '0.25rem 0', fontSize: 12, color: '#888' }}>{b.slug}</p>
            {b.productCount !== undefined && <p style={{ margin: 0, fontSize: 12, color: '#888' }}>{b.productCount} products</p>}
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.75rem', justifyContent: 'center' }}>
              <button onClick={() => { setEditId(b.id); setForm({ name: b.name, slug: b.slug, logoUrl: b.logoUrl ?? '' }); setShowForm(true); }}
                style={{ padding: '0.25rem 0.75rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Edit</button>
              <button onClick={() => handleDelete(b.id)}
                style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
            </div>
          </div>
        ))}
        {brands.length === 0 && <p style={{ color: '#888' }}>No brands yet.</p>}
      </div>
    </div>
  );
}
