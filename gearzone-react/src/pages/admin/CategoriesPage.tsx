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

  const resetForm = () => { setShowForm(false); setEditId(null); setForm({ name: '', slug: '', parentId: '' }); };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      const payload = { name: form.name, slug: form.slug, parentId: form.parentId || undefined };
      if (editId) await adminApi.categories.update(editId, payload);
      else await adminApi.categories.create(payload);
      resetForm(); fetchCategories();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save category.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this category?')) return;
    await adminApi.categories.delete(id);
    fetchCategories();
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  const rootCategories = categories.filter(c => !c.parentId);
  const subCategories = categories.filter(c => !!c.parentId);

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Categories</h2>
        <button className="btn btn-primary" onClick={() => { resetForm(); setShowForm(true); }}>+ Add Category</button>
      </div>

      {showForm && (
        <div className="card shadow-sm mb-4">
          <div className="card-header"><h5 className="mb-0">{editId ? 'Edit Category' : 'New Category'}</h5></div>
          <div className="card-body">
            <form onSubmit={handleSubmit}>
              <div className="row g-3">
                <div className="col-md-4">
                  <label className="form-label">Name</label>
                  <input className="form-control" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required />
                </div>
                <div className="col-md-4">
                  <label className="form-label">Slug</label>
                  <input className="form-control" placeholder="e.g. gaming-mice" value={form.slug} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} required />
                </div>
                <div className="col-md-4">
                  <label className="form-label">Parent Category</label>
                  <select className="form-select" value={form.parentId} onChange={e => setForm(f => ({ ...f, parentId: e.target.value }))}>
                    <option value="">No parent (root)</option>
                    {rootCategories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
              </div>
              {error && <div className="alert alert-danger mt-3 py-2">{error}</div>}
              <div className="d-flex gap-2 mt-3">
                <button type="submit" className="btn btn-success" disabled={saving}>
                  {saving ? <span className="spinner-border spinner-border-sm me-1" /> : null}Save
                </button>
                <button type="button" className="btn btn-outline-secondary" onClick={resetForm}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="card shadow-sm mb-4">
        <div className="card-header"><h5 className="mb-0">Root Categories</h5></div>
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr><th>Name</th><th>Slug</th><th className="text-end">Products</th><th></th></tr>
            </thead>
            <tbody>
              {rootCategories.map(c => (
                <tr key={c.id}>
                  <td className="fw-semibold">{c.name}</td>
                  <td className="text-muted small">{c.slug}</td>
                  <td className="text-end">{c.productCount ?? 0}</td>
                  <td className="text-center">
                    <button className="btn btn-sm btn-outline-primary me-1" onClick={() => { setEditId(c.id); setForm({ name: c.name, slug: c.slug, parentId: c.parentId ?? '' }); setShowForm(true); }}>Edit</button>
                    <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(c.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {subCategories.length > 0 && (
        <div className="card shadow-sm">
          <div className="card-header"><h5 className="mb-0">Sub-categories</h5></div>
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr><th>Name</th><th>Slug</th><th>Parent</th><th></th></tr>
              </thead>
              <tbody>
                {subCategories.map(c => (
                  <tr key={c.id}>
                    <td className="ps-4">↳ {c.name}</td>
                    <td className="text-muted small">{c.slug}</td>
                    <td className="text-muted small">{c.parentName}</td>
                    <td className="text-center">
                      <button className="btn btn-sm btn-outline-primary me-1" onClick={() => { setEditId(c.id); setForm({ name: c.name, slug: c.slug, parentId: c.parentId ?? '' }); setShowForm(true); }}>Edit</button>
                      <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(c.id)}>Delete</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
