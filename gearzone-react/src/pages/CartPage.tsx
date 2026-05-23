import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { cartApi } from '../api/cart';

interface CartItem {
  productSlug: string;
  productName: string;
  imageUrl?: string;
  price: number;
  quantity: number;
  storeName: string;
}

interface Cart {
  items: CartItem[];
  totalPrice?: number;
}

export default function CartPage() {
  const navigate = useNavigate();
  const [cart, setCart] = useState<Cart | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchCart = () => {
    setLoading(true);
    cartApi.get().then(d => setCart(d as Cart)).finally(() => setLoading(false));
  };

  useEffect(() => { fetchCart(); }, []);

  const handleQtyChange = async (slug: string, qty: number) => {
    if (qty < 1) return;
    await cartApi.updateQuantity(slug, qty);
    fetchCart();
  };

  const handleRemove = async (slug: string) => {
    await cartApi.remove(slug);
    fetchCart();
  };

  const total = cart?.items.reduce((sum, i) => sum + i.price * i.quantity, 0) ?? 0;

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <h2 className="mb-4">Your Cart</h2>

      {(!cart?.items || cart.items.length === 0) ? (
        <div className="alert alert-info">
          Your cart is empty. <Link to="/products">Browse products</Link>
        </div>
      ) : (
        <>
          <div className="card shadow-sm mb-4">
            <div className="card-body p-0">
              {cart.items.map((item, idx) => (
                <div key={item.productSlug}
                  className={`d-flex align-items-center gap-3 p-3 ${idx < cart.items.length - 1 ? 'border-bottom' : ''}`}>
                  {item.imageUrl && (
                    <img src={item.imageUrl} alt={item.productName}
                      className="rounded" style={{ width: 80, height: 80, objectFit: 'cover' }} />
                  )}
                  <div className="flex-grow-1">
                    <Link to={`/products/${item.productSlug}`} className="fw-semibold text-dark text-decoration-none">
                      {item.productName}
                    </Link>
                    <p className="text-muted small mb-1">{item.storeName}</p>
                    <p className="text-danger fw-bold mb-0">{item.price?.toLocaleString()} ₫</p>
                  </div>
                  <div className="d-flex align-items-center gap-2">
                    <input type="number" min={1} value={item.quantity}
                      onChange={e => handleQtyChange(item.productSlug, Number(e.target.value))}
                      className="form-control form-control-sm" style={{ width: 65 }} />
                    <button className="btn btn-outline-danger btn-sm"
                      onClick={() => handleRemove(item.productSlug)}>Remove</button>
                  </div>
                  <div className="text-end fw-bold" style={{ minWidth: 110 }}>
                    {(item.price * item.quantity).toLocaleString()} ₫
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="d-flex justify-content-end align-items-center gap-4">
            <span className="fs-5 fw-bold">Total: {total.toLocaleString()} ₫</span>
            <button className="btn btn-success btn-lg" onClick={() => navigate('/checkout')}>
              Proceed to Checkout
            </button>
          </div>
        </>
      )}
    </div>
  );
}
