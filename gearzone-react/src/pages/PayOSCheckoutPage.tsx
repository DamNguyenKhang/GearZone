import { useLocation, useNavigate } from 'react-router-dom';
import { checkoutApi } from '../api/checkout';
import { useState } from 'react';

interface PayOSData {
  qrCode?: string;
  checkoutUrl?: string;
  orderCode?: string | number;
  amount?: number;
  description?: string;
}

export default function PayOSCheckoutPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const payosData = (location.state as { payosData?: PayOSData })?.payosData;
  const [cancelling, setCancelling] = useState(false);

  if (!payosData) {
    navigate('/cart');
    return null;
  }

  const handleCancel = async () => {
    setCancelling(true);
    try {
      if (payosData.orderCode) {
        await checkoutApi.cancelPayment(String(payosData.orderCode));
      }
      navigate('/cart');
    } finally {
      setCancelling(false);
    }
  };

  return (
    <div style={{ padding: '2rem', maxWidth: 500, margin: '4rem auto', textAlign: 'center' }}>
      <h1>Complete Your Payment</h1>
      <p style={{ color: '#555' }}>Scan the QR code or click the link below to pay.</p>

      {payosData.amount && (
        <p style={{ fontSize: 22, fontWeight: 700, color: '#e53e3e' }}>
          {payosData.amount.toLocaleString()} VND
        </p>
      )}
      {payosData.description && <p style={{ color: '#666' }}>{payosData.description}</p>}

      {payosData.qrCode && (
        <img src={payosData.qrCode} alt="Payment QR Code"
          style={{ width: 250, height: 250, margin: '1.5rem auto', display: 'block', border: '1px solid #ccc', borderRadius: 8 }} />
      )}

      {payosData.checkoutUrl && (
        <a href={payosData.checkoutUrl} target="_blank" rel="noopener noreferrer"
          style={{ display: 'inline-block', padding: '0.75rem 2rem', background: '#3182ce', color: '#fff', borderRadius: 6, textDecoration: 'none', fontWeight: 600, marginBottom: '1rem' }}>
          Open Payment Page
        </a>
      )}

      <p style={{ fontSize: 13, color: '#888', marginTop: '1rem' }}>
        After completing payment, you will be redirected automatically.
      </p>

      <button onClick={handleCancel} disabled={cancelling}
        style={{ marginTop: '1rem', padding: '0.5rem 1.5rem', background: 'none', border: '1px solid #ccc', borderRadius: 4, cursor: 'pointer', color: '#555' }}>
        {cancelling ? 'Cancelling…' : 'Cancel Payment'}
      </button>
    </div>
  );
}
