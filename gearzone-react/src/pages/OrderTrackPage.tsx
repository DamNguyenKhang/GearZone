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

const statusBadge: Record<string, string> = {
  Pending: 'warning', Processing: 'primary', Shipping: 'info',
  Delivered: 'success', Cancelled: 'danger',
};

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

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!data) return <div className="container py-4"><div className="alert alert-warning">Order not found.</div></div>;

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-lg-7">
          <h2 className="mb-4">Order Tracking</h2>

          <div className="card shadow-sm mb-4">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <p className="mb-1"><strong>Order:</strong> #{data.subOrderId}</p>
                  {data.trackingNumber && <p className="mb-1"><strong>Tracking #:</strong> {data.trackingNumber}</p>}
                  {data.estimatedDelivery && <p className="mb-1"><strong>Est. Delivery:</strong> {data.estimatedDelivery}</p>}
                  {data.productNames?.length > 0 && (
                    <p className="text-muted small mb-0">{data.productNames.join(', ')}</p>
                  )}
                </div>
                <span className={`badge bg-${statusBadge[data.status] ?? 'secondary'} fs-6`}>{data.status}</span>
              </div>
            </div>
          </div>

          <h5 className="mb-3">Timeline</h5>
          <div className="ps-3 border-start border-2">
            {data.timeline?.map((step, i) => (
              <div key={i} className="mb-4 position-relative">
                <div className="position-absolute bg-primary rounded-circle"
                  style={{ width: 12, height: 12, left: -22, top: 4, border: '2px solid white', boxShadow: '0 0 0 2px #0d6efd' }} />
                <p className="fw-semibold mb-0">{step.status}</p>
                <p className="text-muted small mb-0">{step.description}</p>
                {step.timestamp && <small className="text-secondary">{new Date(step.timestamp).toLocaleString()}</small>}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
