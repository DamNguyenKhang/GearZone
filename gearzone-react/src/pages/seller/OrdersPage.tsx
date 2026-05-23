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
    } finally {
      setProcessing(null);
    }
  };

  const statusColors: Record<string, string> = {
    Pending: '#ed8936', Approved: '#3182ce', Processing: '#805ad5',
    Shipping: '#d69e2e', Delivered: '#38a169', Cancelled: '#e53e3e',
  };

  const actionButtons: Record<string, Array<{ label: string; action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered'; color: string }>> = {
    Pending: [
      { label: 'Approve', action: 'approve', color: '#38a169' },
      { label: 'Reject', action: 'reject', color: '#e53e3e' },
    ],
    Approved: [{ label: 'Mark Processing', action: 'markProcessing', color: '#3182ce' }],
    Processing: [{ label: 'Mark Delivered', action: 'markDelivered', color: '#38a169' }],
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Orders</h1>
      {orders.length === 0 ? <p style={{ color: '#888' }}>No orders yet.</p> : (
        orders.map(order => (
          <div key={order.id} style={{ border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1rem', overflow: 'hidden' }}>
            <div onClick={() => setExpandedId(expandedId === order.id ? null : order.id)}
              style={{ padding: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer', background: '#fafafa' }}>
              <div>
                <span style={{ fontWeight: 600 }}>#{order.id.slice(0, 8)}</span>
                <span style={{ margin: '0 1rem', color: '#888', fontSize: 13 }}>{new Date(order.createdAt).toLocaleDateString()}</span>
                <span style={{ color: '#555', fontSize: 13 }}>{order.buyerName}</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                <span style={{ color: statusColors[order.status] ?? '#333', fontWeight: 600 }}>{order.status}</span>
                <span style={{ fontWeight: 700 }}>{order.totalPrice?.toLocaleString()} VND</span>
              </div>
            </div>

            {expandedId === order.id && (
              <div style={{ padding: '1rem', borderTop: '1px solid #e2e8f0' }}>
                {order.items?.map((item, i) => (
                  <div key={i} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.3rem 0', fontSize: 14 }}>
                    <span>{item.productName} × {item.quantity}</span>
                    <span>{(item.price * item.quantity).toLocaleString()} VND</span>
                  </div>
                ))}
                {actionButtons[order.status] && (
                  <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
                    {actionButtons[order.status].map(btn => (
                      <button key={btn.action} onClick={() => handleAction(order.id, btn.action)}
                        disabled={processing === order.id}
                        style={{ padding: '0.4rem 1rem', background: btn.color, color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
                        {processing === order.id ? '…' : btn.label}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        ))
      )}
    </div>
  );
}
