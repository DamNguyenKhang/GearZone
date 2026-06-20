import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';

export default function HomePage() {
  const [data, setData] = useState<Record<string, unknown> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    catalogApi.home().then(d => setData(d as Record<string, unknown>)).finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  const categories = (data?.categories as Array<{ name: string; href: string }>) ?? [];
  const recommended = ((data as Record<string, { products: Array<{ slug: string; name: string; basePrice: number; imageUrl?: string }> }> | null)?.recommendedRail?.products) ?? [];

  return (
    <div>
      <div className="bg-dark text-white py-5 mb-5">
        <div className="container text-center">
          <h1 className="display-5 fw-bold mb-3">Welcome to GearZone</h1>
          <p className="lead mb-4">Your gaming &amp; PC gear marketplace.</p>
          <Link to="/products" className="btn btn-primary btn-lg">Shop Now</Link>
        </div>
      </div>

      <div className="container">
        {categories.length > 0 && (
          <section className="mb-5">
            <h4 className="mb-3">Browse Categories</h4>
            <div className="d-flex flex-wrap gap-2">
              {categories.slice(0, 8).map(c => (
                <Link key={c.href} to={`/products?categorySlug=${c.href}`}
                  className="btn btn-outline-secondary btn-sm">
                  {c.name}
                </Link>
              ))}
            </div>
          </section>
        )}

        {recommended.length > 0 && (
          <section className="mb-5">
            <h4 className="mb-3">Recommended for You</h4>
            <div className="row row-cols-2 row-cols-md-4 g-3">
              {recommended.map(p => (
                <div key={p.slug} className="col">
                  <Link to={`/products/${p.slug}`} className="text-decoration-none text-dark">
                    <div className="card h-100 shadow-sm">
                      {p.imageUrl
                        ? <img src={p.imageUrl} alt={p.name} className="card-img-top" style={{ height: 160, objectFit: 'cover' }} />
                        : <div className="bg-light" style={{ height: 160 }} />
                      }
                      <div className="card-body py-2 px-3">
                        <p className="card-text small fw-semibold mb-1">{p.name}</p>
                        <p className="text-danger fw-bold mb-0">{p.basePrice?.toLocaleString()} ₫</p>
                      </div>
                    </div>
                  </Link>
                </div>
              ))}
            </div>
          </section>
        )}
      </div>
    </div>
  );
}
