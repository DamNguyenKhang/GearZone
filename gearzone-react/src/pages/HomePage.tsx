import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';

export default function HomePage() {
  const [data, setData] = useState<Record<string, unknown> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    catalogApi.home().then(d => setData(d as Record<string, unknown>)).finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  const categories = (data?.categories as Array<{ name: string; href: string }>) ?? [];
  const recommended = ((data as Record<string, { products: Array<{ slug: string; name: string; basePrice: number; imageUrl?: string }> }> | null)?.recommendedRail?.products) ?? [];

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Welcome to GearZone</h1>
      <p>Your gaming & PC gear marketplace.</p>

      {categories.length > 0 && (
        <section style={{ margin: '2rem 0' }}>
          <h2>Categories</h2>
          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
            {categories.slice(0, 8).map((c) => (
              <Link key={c.href} to={`/products?categorySlug=${c.href}`}
                style={{ padding: '0.5rem 1rem', border: '1px solid #ccc', borderRadius: 4 }}>
                {c.name}
              </Link>
            ))}
          </div>
        </section>
      )}

      {recommended.length > 0 && (
        <section style={{ margin: '2rem 0' }}>
          <h2>Recommended</h2>
          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
            {recommended.slice(0, 6).map((p) => (
              <Link key={p.slug} to={`/products/${p.slug}`}
                style={{ width: 180, border: '1px solid #ccc', borderRadius: 8, overflow: 'hidden', textDecoration: 'none', color: 'inherit' }}>
                {p.imageUrl && <img src={p.imageUrl} alt={p.name} style={{ width: '100%', height: 140, objectFit: 'cover' }} />}
                <div style={{ padding: '0.5rem' }}>
                  <p style={{ margin: 0, fontWeight: 500 }}>{p.name}</p>
                  <p style={{ margin: 0, color: '#e53e3e' }}>{p.basePrice?.toLocaleString()} VND</p>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
