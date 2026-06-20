import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import { useAuth } from '../contexts/AuthContext';

interface StoreProfile {
  slug: string;
  name: string;
  description?: string;
  logoUrl?: string;
  followerCount?: number;
  isFollowing?: boolean;
}

interface Product {
  slug: string;
  name: string;
  basePrice: number;
  imageUrl?: string;
}

export default function StoreProfilePage() {
  const { slug } = useParams<{ slug: string }>();
  const { user } = useAuth();
  const [store, setStore] = useState<StoreProfile | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [following, setFollowing] = useState(false);

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    Promise.all([catalogApi.storeProfile(slug), catalogApi.storeProducts(slug)]).then(([s, p]) => {
      const storeData = s as StoreProfile;
      setStore(storeData);
      setFollowing(storeData.isFollowing ?? false);
      setProducts((p as { items?: Product[] }).items ?? (p as Product[]));
    }).finally(() => setLoading(false));
  }, [slug]);

  const handleFollow = async () => {
    if (!slug) return;
    try { await catalogApi.followStore(slug); setFollowing(f => !f); } catch { }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!store) return <div className="container py-4"><div className="alert alert-warning">Store not found.</div></div>;

  return (
    <div className="container">
      <div className="card shadow-sm mb-4">
        <div className="card-body d-flex align-items-center gap-4">
          {store.logoUrl
            ? <img src={store.logoUrl} alt={store.name} className="rounded-circle" style={{ width: 80, height: 80, objectFit: 'cover' }} />
            : <div className="rounded-circle bg-secondary d-flex align-items-center justify-content-center text-white fw-bold fs-3"
                style={{ width: 80, height: 80 }}>{store.name[0]}</div>
          }
          <div>
            <h3 className="mb-1">{store.name}</h3>
            {store.description && <p className="text-muted mb-1">{store.description}</p>}
            <small className="text-muted">{store.followerCount ?? 0} followers</small>
            {user && (
              <div className="mt-2">
                <button onClick={handleFollow}
                  className={`btn btn-sm ${following ? 'btn-outline-secondary' : 'btn-primary'}`}>
                  {following ? 'Unfollow' : 'Follow'}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      <h4 className="mb-3">Products</h4>
      {products.length === 0 ? (
        <p className="text-muted">No products listed yet.</p>
      ) : (
        <div className="row row-cols-2 row-cols-md-4 g-3">
          {products.map(p => (
            <div key={p.slug} className="col">
              <Link to={`/products/${p.slug}`} className="text-decoration-none text-dark">
                <div className="card h-100 shadow-sm">
                  {p.imageUrl && <img src={p.imageUrl} alt={p.name} className="card-img-top" style={{ height: 140, objectFit: 'cover' }} />}
                  <div className="card-body py-2 px-3">
                    <p className="card-text small fw-semibold mb-1">{p.name}</p>
                    <p className="text-danger small mb-0">{p.basePrice?.toLocaleString()} ₫</p>
                  </div>
                </div>
              </Link>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
