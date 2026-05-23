import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { ordersApi } from '../api/orders';

interface TrackingStep {
  status: string;
  description: string;
  timestamp?: string;
}

interface TrackingData {
  subOrderId: string;
  status: string;
  productNames: string[];
  timeline: TrackingStep[];
  estimatedDelivery?: string;
  trackingNumber?: string;
}

export default function OrderTrackPage() {
  const { subOrderId } = useParams<{ subOrderId: string }>();
  const [data, setData] = useState<TrackingData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!subOrderId) return;
    ordersApi.track(subOrderId)
      .then(d => setData(d as TrackingData))
      .finally(() => setLoading(false));
  }, [subOrderId]);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!data) return <div style={{ padding: '2rem' }}>Order not found.</div>;

  const statusColors: Record<string, string> = {
    Pending: '#ed8936',
    Processing: '#3182ce',
    Shipping: '#805ad5',
    Delivered: '#38a169',
    Cancelled: '#e53e3e',
  };

  return (
    <div style={{ padding: '2rem', maxWidth: 700, margin: '0 auto' }}>
      <h1>Order Tracking</h1>

      <div style={{ padding: '1.25rem', background: '#f9f9f9', borderRadius: 8, marginBottom: '2rem' }}>
        <p style={{ margin: 0 }}><strong>Order:</strong> #{data.subOrderId}</p>
        {data.trackingNumber && <p style={{ margin: '0.25rem 0' }}><strong>Tracking #:</strong> {data.trackingNumber}</p>}
        {data.estimatedDelivery && <p style={{ margin: '0.25rem 0' }}><strong>Est. Delivery:</strong> {data.estimatedDelivery}</p>}
        <p style={{ margin: '0.25rem 0' }}>
          <strong>Status: </strong>
          <span style={{ color: statusColors[data.status] ?? '#333', fontWeight: 700 }}>{data.status}</span>
        </p>
        {data.productNames?.length > 0 && (
          <p style={{ margin: '0.25rem 0', color: '#555', fontSize: 14 }}>{data.productNames.join(', ')}</p>
        )}
      </div>

      <h2>Timeline</h2>
      <div style={{ position: 'relative', paddingLeft: '2rem' }}>
        {data.timeline?.map((step, i) => (
          <div key={i} style={{ position: 'relative', paddingBottom: '1.5rem' }}>
            <div style={{
              position: 'absolute', left: -28, top: 4,
              width: 16, height: 16, borderRadius: '50%',
              background: i === 0 ? '#38a169' : '#ccc',
              border: '2px solid #fff', boxShadow: '0 0 0 2px #ccc'
            }} />
            {i < (data.timeline.length - 1) && (
              <div style={{ position: 'absolute', left: -21, top: 20, width: 2, height: '100%', background: '#e2e8f0' }} />
            )}
            <p style={{ margin: 0, fontWeight: 600 }}>{step.status}</p>
            <p style={{ margin: '0.25rem 0', color: '#555', fontSize: 14 }}>{step.description}</p>
            {step.timestamp && <p style={{ margin: 0, fontSize: 12, color: '#999' }}>{new Date(step.timestamp).toLocaleString()}</p>}
          </div>
        ))}
      </div>
    </div>
  );
}
