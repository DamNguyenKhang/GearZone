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
  const [cartMsgType, setCartMsgType] = useState<'success' | 'danger'>('success');

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
      setCartMsgType('success');
      setTimeout(() => setCartMsg(''), 3000);
    } catch (e: unknown) {
      setCartMsg(e instanceof Error ? e.message : 'Failed to add to cart.');
      setCartMsgType('danger');
    } finally {
      setAddingToCart(false);
    }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!product) return <div className="container py-4"><div className="alert alert-warning">Product not found.</div></div>;

  return (
    <div className="container">
      <div className="row g-4 mb-5">
        <div className="col-md-5">
          {product.imageUrl
            ? <img src={product.imageUrl} alt={product.name} className="img-fluid rounded shadow-sm" />
            : <div className="bg-light rounded" style={{ height: 350 }} />
          }
        </div>

        <div className="col-md-7">
          <h2 className="mb-1">{product.name}</h2>
          <p className="text-muted mb-2">{product.brandName} · <Link to={`/stores/${product.storeSlug}`}>{product.storeName}</Link></p>
          {product.averageRating !== undefined && (
            <p className="text-muted small mb-2">
              {'★'.repeat(Math.round(product.averageRating))}{'☆'.repeat(5 - Math.round(product.averageRating))}
              {' '}{product.averageRating.toFixed(1)} ({product.reviewCount} reviews)
            </p>
          )}
          <h3 className="text-danger fw-bold mb-3">{product.basePrice?.toLocaleString()} ₫</h3>

          <div className="d-flex align-items-center gap-3 mb-3">
            <label className="fw-semibold mb-0">Qty:</label>
            <input type="number" min={1} value={quantity}
              onChange={e => setQuantity(Number(e.target.value))}
              className="form-control" style={{ width: 80 }} />
            <button className="btn btn-primary px-4" onClick={handleAddToCart} disabled={addingToCart}>
              {addingToCart ? <span className="spinner-border spinner-border-sm me-1" /> : null}
              Add to Cart
            </button>
          </div>

          {cartMsg && <div className={`alert alert-${cartMsgType} py-2`}>{cartMsg}</div>}

          {product.description && (
            <div className="mt-3">
              <h5>Description</h5>
              <p className="text-muted" style={{ lineHeight: 1.7 }}>{product.description}</p>
            </div>
          )}

          {product.specifications && Object.keys(product.specifications).length > 0 && (
            <div className="mt-3">
              <h5>Specifications</h5>
              <table className="table table-sm table-bordered">
                <tbody>
                  {Object.entries(product.specifications).map(([k, v]) => (
                    <tr key={k}>
                      <td className="fw-semibold text-muted" style={{ width: '40%' }}>{k}</td>
                      <td>{v}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {product.reviews && product.reviews.length > 0 && (
        <section className="mb-5">
          <h4 className="mb-3">Customer Reviews</h4>
          {product.reviews.map(r => (
            <div key={r.id} className="card mb-2">
              <div className="card-body py-3">
                <p className="mb-1 fw-semibold">
                  {r.reviewerName}
                  <span className="text-warning ms-2">{'★'.repeat(r.rating)}{'☆'.repeat(5 - r.rating)}</span>
                </p>
                <p className="mb-1 text-muted">{r.comment}</p>
                <p className="mb-0 small text-secondary">{new Date(r.createdAt).toLocaleDateString()}</p>
              </div>
            </div>
          ))}
        </section>
      )}

      {product.relatedProducts && product.relatedProducts.length > 0 && (
        <section className="mb-5">
          <h4 className="mb-3">Related Products</h4>
          <div className="row row-cols-2 row-cols-md-4 g-3">
            {product.relatedProducts.map(p => (
              <div key={p.slug} className="col">
                <Link to={`/products/${p.slug}`} className="text-decoration-none text-dark">
                  <div className="card h-100 shadow-sm">
                    {p.imageUrl && <img src={p.imageUrl} alt={p.name} className="card-img-top" style={{ height: 130, objectFit: 'cover' }} />}
                    <div className="card-body py-2 px-3">
                      <p className="card-text small fw-semibold mb-1">{p.name}</p>
                      <p className="text-danger small mb-0">{p.basePrice?.toLocaleString()} ₫</p>
                    </div>
                  </div>
                </Link>
              </div>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
