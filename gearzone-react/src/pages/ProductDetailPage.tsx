import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import { cartApi } from '../api/cart';
import { useAuth } from '../contexts/AuthContext';

interface ProductDetail {
  slug: string;
  name: string;
  basePrice: number;
  imageUrl?: string;
  imageUrls?: string[];
  brandName: string;
  storeName: string;
  storeSlug: string;
  description?: string;
  specifications?: Record<string, string>;
  attributes?: Array<{ name: string; values: string[] }>;
  reviews?: Array<{ id: string; rating: number; comment: string; reviewerName: string; createdAt: string }>;
  relatedProducts?: Array<{ slug: string; name: string; basePrice: number; imageUrl?: string }>;
  averageRating?: number;
  reviewCount?: number;
}

export default function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [addingToCart, setAddingToCart] = useState(false);
  const [cartMsg, setCartMsg] = useState('');

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    catalogApi.getProduct(slug)
      .then(d => setProduct(d as ProductDetail))
      .finally(() => setLoading(false));
  }, [slug]);

  const handleAddToCart = async () => {
    if (!user) { navigate('/login'); return; }
    if (!product) return;
    setAddingToCart(true);
    try {
      await cartApi.add(product.slug, quantity);
      setCartMsg('Added to cart!');
      setTimeout(() => setCartMsg(''), 3000);
    } catch (e: unknown) {
      setCartMsg(e instanceof Error ? e.message : 'Failed to add to cart.');
    } finally {
      setAddingToCart(false);
    }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!product) return <div style={{ padding: '2rem' }}>Product not found.</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 1000, margin: '0 auto' }}>
      <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
        <div style={{ flex: '0 0 400px' }}>
          {product.imageUrl && (
            <img src={product.imageUrl} alt={product.name} style={{ width: '100%', borderRadius: 8, objectFit: 'cover' }} />
          )}
        </div>

        <div style={{ flex: 1 }}>
          <h1 style={{ marginTop: 0 }}>{product.name}</h1>
          <p style={{ color: '#e53e3e', fontSize: 24, fontWeight: 700 }}>{product.basePrice?.toLocaleString()} VND</p>
          <p style={{ color: '#666' }}>{product.brandName} · <Link to={`/stores/${product.storeSlug}`}>{product.storeName}</Link></p>
          {product.averageRating !== undefined && (
            <p style={{ color: '#888' }}>Rating: {product.averageRating.toFixed(1)} / 5 ({product.reviewCount} reviews)</p>
          )}

          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', margin: '1rem 0' }}>
            <label>Qty:</label>
            <input type="number" min={1} value={quantity} onChange={e => setQuantity(Number(e.target.value))}
              style={{ width: 60, padding: '0.25rem' }} />
            <button onClick={handleAddToCart} disabled={addingToCart}
              style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              {addingToCart ? 'Adding…' : 'Add to Cart'}
            </button>
          </div>
          {cartMsg && <p style={{ color: cartMsg.includes('Failed') ? 'red' : 'green' }}>{cartMsg}</p>}

          {product.description && (
            <div style={{ marginTop: '1rem' }}>
              <h3>Description</h3>
              <p style={{ lineHeight: 1.6 }}>{product.description}</p>
            </div>
          )}

          {product.specifications && Object.keys(product.specifications).length > 0 && (
            <div style={{ marginTop: '1rem' }}>
              <h3>Specifications</h3>
              <table style={{ borderCollapse: 'collapse', width: '100%' }}>
                <tbody>
                  {Object.entries(product.specifications).map(([k, v]) => (
                    <tr key={k} style={{ borderBottom: '1px solid #eee' }}>
                      <td style={{ padding: '0.4rem', fontWeight: 600, width: '40%' }}>{k}</td>
                      <td style={{ padding: '0.4rem' }}>{v}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {product.reviews && product.reviews.length > 0 && (
        <section style={{ marginTop: '3rem' }}>
          <h2>Reviews</h2>
          {product.reviews.map(r => (
            <div key={r.id} style={{ padding: '1rem', borderBottom: '1px solid #eee' }}>
              <p style={{ margin: 0, fontWeight: 600 }}>{r.reviewerName} — {'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</p>
              <p style={{ margin: '0.25rem 0', color: '#555' }}>{r.comment}</p>
              <p style={{ margin: 0, fontSize: 12, color: '#999' }}>{new Date(r.createdAt).toLocaleDateString()}</p>
            </div>
          ))}
        </section>
      )}

      {product.relatedProducts && product.relatedProducts.length > 0 && (
        <section style={{ marginTop: '3rem' }}>
          <h2>Related Products</h2>
          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
            {product.relatedProducts.map(p => (
              <Link key={p.slug} to={`/products/${p.slug}`}
                style={{ width: 160, border: '1px solid #ccc', borderRadius: 8, overflow: 'hidden', textDecoration: 'none', color: 'inherit' }}>
                {p.imageUrl && <img src={p.imageUrl} alt={p.name} style={{ width: '100%', height: 120, objectFit: 'cover' }} />}
                <div style={{ padding: '0.5rem' }}>
                  <p style={{ margin: 0, fontSize: 13, fontWeight: 500 }}>{p.name}</p>
                  <p style={{ margin: 0, color: '#e53e3e', fontSize: 13 }}>{p.basePrice?.toLocaleString()} VND</p>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
