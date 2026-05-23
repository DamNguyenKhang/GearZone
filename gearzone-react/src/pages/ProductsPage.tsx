import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';

interface Product {
  slug: string;
  name: string;
  basePrice: number;
  imageUrl?: string;
  brandName: string;
  storeName: string;
}

interface PagedResult {
  items: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export default function ProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [result, setResult] = useState<PagedResult | null>(null);
  const [loading, setLoading] = useState(true);

  const page = Number(searchParams.get('page') ?? '1');
  const search = searchParams.get('search') ?? '';
  const categorySlug = searchParams.get('categorySlug') ?? undefined;

  useEffect(() => {
    setLoading(true);
    catalogApi.browseProducts({ page, search, categorySlug, pageSize: 12 })
      .then(d => setResult(d as PagedResult))
      .finally(() => setLoading(false));
  }, [page, search, categorySlug]);

  const totalPages = result ? Math.ceil(result.totalCount / result.pageSize) : 1;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Products</h1>

      <input
        placeholder="Search products…"
        defaultValue={search}
        onKeyDown={e => {
          if (e.key === 'Enter') {
            setSearchParams({ search: (e.target as HTMLInputElement).value, page: '1' });
          }
        }}
        style={{ padding: '0.5rem', width: '100%', maxWidth: 400, marginBottom: '1rem' }}
      />

      {loading ? <div>Loading…</div> : (
        <>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem' }}>
            {result?.items.map(p => (
              <Link key={p.slug} to={`/products/${p.slug}`}
                style={{ width: 200, border: '1px solid #ccc', borderRadius: 8, overflow: 'hidden', textDecoration: 'none', color: 'inherit' }}>
                {p.imageUrl && <img src={p.imageUrl} alt={p.name} style={{ width: '100%', height: 160, objectFit: 'cover' }} />}
                <div style={{ padding: '0.75rem' }}>
                  <p style={{ margin: 0, fontWeight: 600, fontSize: 14 }}>{p.name}</p>
                  <p style={{ margin: '0.25rem 0', color: '#e53e3e', fontWeight: 700 }}>{p.basePrice?.toLocaleString()} VND</p>
                  <p style={{ margin: 0, fontSize: 12, color: '#666' }}>{p.brandName} · {p.storeName}</p>
                </div>
              </Link>
            ))}
          </div>

          <div style={{ marginTop: '2rem', display: 'flex', gap: '0.5rem' }}>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map(n => (
              <button key={n} onClick={() => setSearchParams({ page: String(n), search, ...(categorySlug ? { categorySlug } : {}) })}
                style={{ fontWeight: n === page ? 'bold' : 'normal' }}>
                {n}
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
