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

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 800, margin: '0 auto' }}>
      <h1>Your Cart</h1>

      {(!cart?.items || cart.items.length === 0) ? (
        <p>Your cart is empty. <Link to="/products">Browse products</Link></p>
      ) : (
        <>
          {cart.items.map(item => (
            <div key={item.productSlug} style={{ display: 'flex', gap: '1rem', padding: '1rem 0', borderBottom: '1px solid #eee', alignItems: 'center' }}>
              {item.imageUrl && <img src={item.imageUrl} alt={item.productName} style={{ width: 80, height: 80, objectFit: 'cover', borderRadius: 4 }} />}
              <div style={{ flex: 1 }}>
                <Link to={`/products/${item.productSlug}`} style={{ fontWeight: 600, color: 'inherit' }}>{item.productName}</Link>
                <p style={{ margin: '0.25rem 0', fontSize: 13, color: '#888' }}>{item.storeName}</p>
                <p style={{ margin: 0, color: '#e53e3e' }}>{item.price?.toLocaleString()} VND</p>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <input type="number" min={1} value={item.quantity}
                  onChange={e => handleQtyChange(item.productSlug, Number(e.target.value))}
                  style={{ width: 55, padding: '0.25rem' }} />
                <button onClick={() => handleRemove(item.productSlug)}
                  style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>
                  Remove
                </button>
              </div>
              <p style={{ fontWeight: 700, minWidth: 100, textAlign: 'right' }}>{(item.price * item.quantity).toLocaleString()} VND</p>
            </div>
          ))}

          <div style={{ display: 'flex', justifyContent: 'flex-end', alignItems: 'center', gap: '1.5rem', marginTop: '1.5rem' }}>
            <span style={{ fontSize: 18, fontWeight: 700 }}>Total: {total.toLocaleString()} VND</span>
            <button onClick={() => navigate('/checkout')}
              style={{ padding: '0.75rem 2rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 16, fontWeight: 600 }}>
              Proceed to Checkout
            </button>
          </div>
        </>
      )}
    </div>
  );
}
