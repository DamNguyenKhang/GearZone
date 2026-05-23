import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Category {
  id: string;
  name: string;
  slug: string;
  parentId?: string;
  parentName?: string;
  productCount?: number;
}

export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', slug: '', parentId: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchCategories = () => {
    adminApi.categories.list()
      .then(d => setCategories((d as { items?: Category[] }).items ?? (d as Category[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchCategories(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      const payload = { name: form.name, slug: form.slug, parentId: form.parentId || undefined };
      if (editId) {
        await adminApi.categories.update(editId, payload);
      } else {
        await adminApi.categories.create(payload);
      }
      setShowForm(false); setEditId(null); setForm({ name: '', slug: '', parentId: '' });
      fetchCategories();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save category.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this category?')) return;
    await adminApi.categories.delete(id);
    fetchCategories();
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  const rootCategories = categories.filter(c => !c.parentId);
  const subCategories = categories.filter(c => !!c.parentId);

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Categories</h1>
        <button onClick={() => { setShowForm(true); setEditId(null); setForm({ name: '', slug: '', parentId: '' }); }}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          + Add Category
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem', maxWidth: 400 }}>
          <h3 style={{ margin: 0 }}>{editId ? 'Edit Category' : 'New Category'}</h3>
          <input placeholder="Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <input placeholder="Slug (e.g. gaming-mice)" value={form.slug} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <select value={form.parentId} onChange={e => setForm(f => ({ ...f, parentId: e.target.value }))} style={{ padding: '0.5rem' }}>
            <option value="">No parent (root category)</option>
            {rootCategories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </form>
      )}

      <h2>Root Categories</h2>
      <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: '2rem' }}>
        <thead><tr style={{ background: '#f0f0f0' }}>
          <th style={{ padding: '0.75rem', textAlign: 'left' }}>Name</th>
          <th style={{ padding: '0.75rem', textAlign: 'left' }}>Slug</th>
          <th style={{ padding: '0.75rem', textAlign: 'right' }}>Products</th>
          <th style={{ padding: '0.75rem' }}></th>
        </tr></thead>
        <tbody>
          {rootCategories.map(c => (
            <tr key={c.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', fontWeight: 500 }}>{c.name}</td>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{c.slug}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>{c.productCount ?? 0}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <button onClick={() => { setEditId(c.id); setForm({ name: c.name, slug: c.slug, parentId: c.parentId ?? '' }); setShowForm(true); }} style={{ marginRight: '0.5rem', padding: '0.25rem 0.75rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Edit</button>
                <button onClick={() => handleDelete(c.id)} style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {subCategories.length > 0 && (
        <>
          <h2>Sub-categories</h2>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead><tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Name</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Slug</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Parent</th>
              <th style={{ padding: '0.75rem' }}></th>
            </tr></thead>
            <tbody>
              {subCategories.map(c => (
                <tr key={c.id} style={{ borderBottom: '1px solid #eee' }}>
                  <td style={{ padding: '0.75rem', paddingLeft: '1.5rem' }}>↳ {c.name}</td>
                  <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{c.slug}</td>
                  <td style={{ padding: '0.75rem', color: '#555', fontSize: 13 }}>{c.parentName}</td>
                  <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                    <button onClick={() => { setEditId(c.id); setForm({ name: c.name, slug: c.slug, parentId: c.parentId ?? '' }); setShowForm(true); }} style={{ marginRight: '0.5rem', padding: '0.25rem 0.75rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Edit</button>
                    <button onClick={() => handleDelete(c.id)} style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
