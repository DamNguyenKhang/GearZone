import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { checkoutApi } from '../api/checkout';

interface Address {
  id: string;
  fullName: string;
  phone: string;
  addressLine: string;
  province: string;
  district: string;
  ward: string;
}

interface CheckoutData {
  items: Array<{
    productSlug: string;
    productName: string;
    price: number;
    quantity: number;
    storeName: string;
  }>;
  addresses: Address[];
  totalPrice: number;
}

export default function CheckoutPage() {
  const navigate = useNavigate();
  const [data, setData] = useState<CheckoutData | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedAddress, setSelectedAddress] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<'cod' | 'payos'>('cod');
  const [voucherCode, setVoucherCode] = useState('');
  const [voucherMsg, setVoucherMsg] = useState('');
  const [discount, setDiscount] = useState(0);
  const [placing, setPlacing] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    checkoutApi.getData()
      .then(d => {
        const cd = d as CheckoutData;
        setData(cd);
        if (cd.addresses?.length > 0) setSelectedAddress(cd.addresses[0].id);
      })
      .finally(() => setLoading(false));
  }, []);

  const handleApplyVoucher = async () => {
    if (!voucherCode) return;
    try {
      const result = await checkoutApi.applyVoucher(voucherCode) as { discount?: number; message?: string };
      setDiscount(result.discount ?? 0);
      setVoucherMsg(result.message ?? 'Voucher applied!');
    } catch (e: unknown) {
      setVoucherMsg(e instanceof Error ? e.message : 'Invalid voucher.');
      setDiscount(0);
    }
  };

  const handlePlaceOrder = async () => {
    if (!selectedAddress) { setError('Please select a delivery address.'); return; }
    setPlacing(true); setError('');
    try {
      const result = await checkoutApi.placeOrder({
        addressId: selectedAddress,
        paymentMethod,
        voucherCode: voucherCode || undefined,
      }) as { orderId?: string; payosData?: unknown };

      if (paymentMethod === 'payos' && result.payosData) {
        navigate('/checkout/payos', { state: { payosData: result.payosData } });
      } else {
        navigate('/order-success', { state: { orderId: result.orderId } });
      }
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to place order.');
    } finally {
      setPlacing(false);
    }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!data) return <div style={{ padding: '2rem' }}>Could not load checkout data.</div>;

  const total = (data.totalPrice ?? 0) - discount;

  return (
    <div style={{ padding: '2rem', maxWidth: 800, margin: '0 auto' }}>
      <h1>Checkout</h1>

      <section style={{ marginBottom: '2rem' }}>
        <h2>Delivery Address</h2>
        {data.addresses.length === 0 ? (
          <p>No address saved. Please add one in your profile.</p>
        ) : (
          data.addresses.map(a => (
            <label key={a.id} style={{ display: 'block', padding: '0.75rem', border: `2px solid ${selectedAddress === a.id ? '#3182ce' : '#ccc'}`, borderRadius: 6, marginBottom: '0.5rem', cursor: 'pointer' }}>
              <input type="radio" name="address" value={a.id} checked={selectedAddress === a.id}
                onChange={() => setSelectedAddress(a.id)} style={{ marginRight: '0.5rem' }} />
              <strong>{a.fullName}</strong> · {a.phone}<br />
              <span style={{ color: '#555', fontSize: 14 }}>{a.addressLine}, {a.ward}, {a.district}, {a.province}</span>
            </label>
          ))
        )}
      </section>

      <section style={{ marginBottom: '2rem' }}>
        <h2>Order Summary</h2>
        {data.items.map((item, i) => (
          <div key={i} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.4rem 0', borderBottom: '1px solid #f0f0f0' }}>
            <span>{item.productName} × {item.quantity} <span style={{ color: '#888', fontSize: 13 }}>({item.storeName})</span></span>
            <span>{(item.price * item.quantity).toLocaleString()} VND</span>
          </div>
        ))}
      </section>

      <section style={{ marginBottom: '2rem' }}>
        <h2>Voucher</h2>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <input placeholder="Enter voucher code" value={voucherCode} onChange={e => setVoucherCode(e.target.value)}
            style={{ flex: 1, padding: '0.5rem' }} />
          <button onClick={handleApplyVoucher} style={{ padding: '0.5rem 1rem' }}>Apply</button>
        </div>
        {voucherMsg && <p style={{ color: discount > 0 ? 'green' : 'red', fontSize: 14 }}>{voucherMsg}</p>}
      </section>

      <section style={{ marginBottom: '2rem' }}>
        <h2>Payment Method</h2>
        <label style={{ marginRight: '1.5rem', cursor: 'pointer' }}>
          <input type="radio" name="payment" value="cod" checked={paymentMethod === 'cod'} onChange={() => setPaymentMethod('cod')} />
          {' '}Cash on Delivery
        </label>
        <label style={{ cursor: 'pointer' }}>
          <input type="radio" name="payment" value="payos" checked={paymentMethod === 'payos'} onChange={() => setPaymentMethod('payos')} />
          {' '}PayOS (Online)
        </label>
      </section>

      <div style={{ textAlign: 'right', marginBottom: '1rem' }}>
        {discount > 0 && <p style={{ color: 'green' }}>Discount: -{discount.toLocaleString()} VND</p>}
        <p style={{ fontSize: 20, fontWeight: 700 }}>Total: {total.toLocaleString()} VND</p>
      </div>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      <button onClick={handlePlaceOrder} disabled={placing}
        style={{ width: '100%', padding: '0.875rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 6, fontSize: 16, fontWeight: 600, cursor: 'pointer' }}>
        {placing ? 'Placing Order…' : 'Place Order'}
      </button>
    </div>
  );
}
