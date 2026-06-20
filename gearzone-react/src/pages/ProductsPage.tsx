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
    <div className="container">
      <h2 className="mb-3">Products</h2>

      <div className="mb-4">
        <input
          className="form-control w-auto d-inline-block"
          style={{ minWidth: 300 }}
          placeholder="Search products…"
          defaultValue={search}
          onKeyDown={e => {
            if (e.key === 'Enter') {
              setSearchParams({ search: (e.target as HTMLInputElement).value, page: '1' });
            }
          }}
        />
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" />
        </div>
      ) : (
        <>
          <div className="row row-cols-2 row-cols-md-3 row-cols-lg-4 g-3 mb-4">
            {result?.items.map(p => (
              <div key={p.slug} className="col">
                <Link to={`/products/${p.slug}`} className="text-decoration-none text-dark">
                  <div className="card h-100 shadow-sm">
                    {p.imageUrl
                      ? <img src={p.imageUrl} alt={p.name} className="card-img-top" style={{ height: 180, objectFit: 'cover' }} />
                      : <div className="bg-light" style={{ height: 180 }} />
                    }
                    <div className="card-body py-2 px-3">
                      <p className="card-text fw-semibold mb-1 small">{p.name}</p>
                      <p className="text-danger fw-bold mb-1">{p.basePrice?.toLocaleString()} ₫</p>
                      <p className="text-muted small mb-0">{p.brandName} · {p.storeName}</p>
                    </div>
                  </div>
                </Link>
              </div>
            ))}
          </div>

          {totalPages > 1 && (
            <nav>
              <ul className="pagination">
                {Array.from({ length: totalPages }, (_, i) => i + 1).map(n => (
                  <li key={n} className={`page-item ${n === page ? 'active' : ''}`}>
                    <button className="page-link"
                      onClick={() => setSearchParams({ page: String(n), search, ...(categorySlug ? { categorySlug } : {}) })}>
                      {n}
                    </button>
                  </li>
                ))}
              </ul>
            </nav>
          )}
        </>
      )}
    </div>
  );
}
