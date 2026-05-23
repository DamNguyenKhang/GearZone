import { useLocation, Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { checkoutApi } from '../api/checkout';

interface OrderSuccess {
  orderId: string;
  totalPrice: number;
  items: Array<{ productName: string; quantity: number; price: number }>;
  deliveryAddress?: string;
}

export default function OrderSuccessPage() {
  const location = useLocation();
  const orderId = (location.state as { orderId?: string })?.orderId;
  const [order, setOrder] = useState<OrderSuccess | null>(null);
  const [loading, setLoading] = useState(!!orderId);

  useEffect(() => {
    if (!orderId) return;
    checkoutApi.getSuccess(orderId)
      .then(d => setOrder(d as OrderSuccess))
      .finally(() => setLoading(false));
  }, [orderId]);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 600, margin: '4rem auto', textAlign: 'center' }}>
      <div style={{ fontSize: 64 }}>✓</div>
      <h1 style={{ color: '#38a169' }}>Order Placed Successfully!</h1>
      <p style={{ color: '#555' }}>Thank you for your purchase. We'll process your order shortly.</p>

      {order && (
        <div style={{ textAlign: 'left', background: '#f9f9f9', padding: '1.5rem', borderRadius: 8, margin: '1.5rem 0' }}>
          <p><strong>Order ID:</strong> {order.orderId}</p>
          {order.deliveryAddress && <p><strong>Delivery to:</strong> {order.deliveryAddress}</p>}
          <hr />
          {order.items?.map((item, i) => (
            <div key={i} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.3rem 0' }}>
              <span>{item.productName} × {item.quantity}</span>
              <span>{(item.price * item.quantity).toLocaleString()} VND</span>
            </div>
          ))}
          {order.totalPrice && (
            <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700, marginTop: '0.5rem', borderTop: '1px solid #e2e8f0', paddingTop: '0.5rem' }}>
              <span>Total</span>
              <span>{order.totalPrice.toLocaleString()} VND</span>
            </div>
          )}
        </div>
      )}

      <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center', marginTop: '1.5rem' }}>
        <Link to="/profile" style={{ padding: '0.6rem 1.5rem', background: '#3182ce', color: '#fff', borderRadius: 6, textDecoration: 'none', fontWeight: 600 }}>
          View My Orders
        </Link>
        <Link to="/products" style={{ padding: '0.6rem 1.5rem', border: '1px solid #ccc', borderRadius: 6, textDecoration: 'none', color: '#333' }}>
          Continue Shopping
        </Link>
      </div>
    </div>
  );
}
