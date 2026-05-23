import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface SubOrder {
  id: string;
  status: string;
  totalPrice: number;
  createdAt: string;
  buyerName: string;
  items: Array<{ productName: string; quantity: number; price: number }>;
}

const statusBadge: Record<string, string> = {
  Pending: 'warning', Approved: 'primary', Processing: 'info',
  Shipping: 'secondary', Delivered: 'success', Cancelled: 'danger',
};

const actionButtons: Record<string, Array<{ label: string; action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered'; variant: string }>> = {
  Pending: [
    { label: 'Approve', action: 'approve', variant: 'success' },
    { label: 'Reject', action: 'reject', variant: 'danger' },
  ],
  Approved: [{ label: 'Mark Processing', action: 'markProcessing', variant: 'primary' }],
  Processing: [{ label: 'Mark Delivered', action: 'markDelivered', variant: 'success' }],
};

export default function SellerOrdersPage() {
  const [orders, setOrders] = useState<SubOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchOrders = () => {
    setLoading(true);
    sellerApi.orders.list()
      .then(d => setOrders((d as { items?: SubOrder[] }).items ?? (d as SubOrder[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchOrders(); }, []);

  const handleAction = async (id: string, action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered') => {
    setProcessing(id);
    try {
      const actions = {
        approve: () => sellerApi.orders.approve(id),
        reject: () => sellerApi.orders.reject(id, 'Rejected by seller'),
        markProcessing: () => sellerApi.orders.markProcessing(id),
        markDelivered: () => sellerApi.orders.markDelivered(id),
      };
      await actions[action]();
      fetchOrders();
    } finally { setProcessing(null); }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <h2 className="mb-4">Orders</h2>

      {orders.length === 0 ? (
        <p className="text-muted">No orders yet.</p>
      ) : (
        <div className="accordion" id="ordersAccordion">
          {orders.map(order => (
            <div key={order.id} className="accordion-item mb-2 border rounded shadow-sm">
              <div className="accordion-header">
                <button
                  className={`accordion-button ${expandedId !== order.id ? 'collapsed' : ''} bg-light`}
                  onClick={() => setExpandedId(expandedId === order.id ? null : order.id)}
                  type="button">
                  <div className="d-flex justify-content-between w-100 me-3 align-items-center">
                    <div>
                      <span className="fw-semibold font-monospace">#{order.id.slice(0, 8)}</span>
                      <span className="text-muted small ms-3">{new Date(order.createdAt).toLocaleDateString()}</span>
                      <span className="text-muted small ms-3">{order.buyerName}</span>
                    </div>
                    <div className="d-flex align-items-center gap-3">
                      <span className={`badge bg-${statusBadge[order.status] ?? 'secondary'}`}>{order.status}</span>
                      <span className="fw-bold">{order.totalPrice?.toLocaleString()} ₫</span>
                    </div>
                  </div>
                </button>
              </div>

              {expandedId === order.id && (
                <div className="accordion-body">
                  {order.items?.map((item, i) => (
                    <div key={i} className="d-flex justify-content-between small border-bottom py-1">
                      <span>{item.productName} × {item.quantity}</span>
                      <span>{(item.price * item.quantity).toLocaleString()} ₫</span>
                    </div>
                  ))}
                  {actionButtons[order.status] && (
                    <div className="d-flex gap-2 mt-3">
                      {actionButtons[order.status].map(btn => (
                        <button key={btn.action}
                          className={`btn btn-sm btn-${btn.variant}`}
                          onClick={() => handleAction(order.id, btn.action)}
                          disabled={processing === order.id}>
                          {processing === order.id ? <span className="spinner-border spinner-border-sm me-1" /> : null}
                          {btn.label}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
