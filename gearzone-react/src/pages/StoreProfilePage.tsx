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
    Promise.all([
      catalogApi.storeProfile(slug),
      catalogApi.storeProducts(slug),
    ]).then(([s, p]) => {
      const storeData = s as StoreProfile;
      setStore(storeData);
      setFollowing(storeData.isFollowing ?? false);
      setProducts((p as { items?: Product[] }).items ?? (p as Product[]));
    }).finally(() => setLoading(false));
  }, [slug]);

  const handleFollow = async () => {
    if (!slug) return;
    try {
      await catalogApi.followStore(slug);
      setFollowing(f => !f);
    } catch { }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!store) return <div style={{ padding: '2rem' }}>Store not found.</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 1000, margin: '0 auto' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', marginBottom: '2rem' }}>
        {store.logoUrl && <img src={store.logoUrl} alt={store.name} style={{ width: 80, height: 80, borderRadius: '50%', objectFit: 'cover' }} />}
        <div>
          <h1 style={{ margin: 0 }}>{store.name}</h1>
          {store.description && <p style={{ color: '#555', margin: '0.25rem 0' }}>{store.description}</p>}
          <p style={{ color: '#888', margin: '0.25rem 0', fontSize: 14 }}>{store.followerCount ?? 0} followers</p>
          {user && (
            <button onClick={handleFollow}
              style={{ padding: '0.4rem 1rem', background: following ? '#eee' : '#3182ce', color: following ? '#333' : '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              {following ? 'Unfollow' : 'Follow'}
            </button>
          )}
        </div>
      </div>

      <h2>Products</h2>
      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
        {products.map(p => (
          <Link key={p.slug} to={`/products/${p.slug}`}
            style={{ width: 180, border: '1px solid #ccc', borderRadius: 8, overflow: 'hidden', textDecoration: 'none', color: 'inherit' }}>
            {p.imageUrl && <img src={p.imageUrl} alt={p.name} style={{ width: '100%', height: 140, objectFit: 'cover' }} />}
            <div style={{ padding: '0.5rem' }}>
              <p style={{ margin: 0, fontWeight: 500, fontSize: 13 }}>{p.name}</p>
              <p style={{ margin: 0, color: '#e53e3e', fontSize: 13 }}>{p.basePrice?.toLocaleString()} VND</p>
            </div>
          </Link>
        ))}
        {products.length === 0 && <p style={{ color: '#888' }}>No products listed yet.</p>}
      </div>
    </div>
  );
}
