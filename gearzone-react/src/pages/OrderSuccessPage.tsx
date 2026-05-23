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

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-6 text-center">
          <div className="display-1 text-success mb-3">✓</div>
          <h2 className="text-success mb-2">Order Placed Successfully!</h2>
          <p className="text-muted mb-4">Thank you for your purchase. We'll process your order shortly.</p>

          {order && (
            <div className="card shadow-sm mb-4 text-start">
              <div className="card-body">
                <p className="mb-1"><strong>Order ID:</strong> {order.orderId}</p>
                {order.deliveryAddress && <p className="mb-2"><strong>Delivery to:</strong> {order.deliveryAddress}</p>}
                <hr />
                {order.items?.map((item, i) => (
                  <div key={i} className="d-flex justify-content-between small mb-1">
                    <span>{item.productName} × {item.quantity}</span>
                    <span>{(item.price * item.quantity).toLocaleString()} ₫</span>
                  </div>
                ))}
                {order.totalPrice && (
                  <div className="d-flex justify-content-between fw-bold mt-2 pt-2 border-top">
                    <span>Total</span>
                    <span className="text-danger">{order.totalPrice.toLocaleString()} ₫</span>
                  </div>
                )}
              </div>
            </div>
          )}

          <div className="d-flex gap-3 justify-content-center">
            <Link to="/profile" className="btn btn-primary">View My Orders</Link>
            <Link to="/products" className="btn btn-outline-secondary">Continue Shopping</Link>
          </div>
        </div>
      </div>
    </div>
  );
}
