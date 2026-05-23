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
  items: Array<{ productSlug: string; productName: string; price: number; quantity: number; storeName: string }>;
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
  const [voucherOk, setVoucherOk] = useState(false);
  const [discount, setDiscount] = useState(0);
  const [placing, setPlacing] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    checkoutApi.getData().then(d => {
      const cd = d as CheckoutData;
      setData(cd);
      if (cd.addresses?.length > 0) setSelectedAddress(cd.addresses[0].id);
    }).finally(() => setLoading(false));
  }, []);

  const handleApplyVoucher = async () => {
    if (!voucherCode) return;
    try {
      const result = await checkoutApi.applyVoucher(voucherCode) as { discount?: number; message?: string };
      setDiscount(result.discount ?? 0);
      setVoucherMsg(result.message ?? 'Voucher applied!');
      setVoucherOk(true);
    } catch (e: unknown) {
      setVoucherMsg(e instanceof Error ? e.message : 'Invalid voucher.');
      setVoucherOk(false);
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

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!data) return <div className="container py-4"><div className="alert alert-danger">Could not load checkout data.</div></div>;

  const total = (data.totalPrice ?? 0) - discount;

  return (
    <div className="container">
      <div className="row g-4">
        <div className="col-lg-8">
          <div className="card shadow-sm mb-3">
            <div className="card-header"><h5 className="mb-0">Delivery Address</h5></div>
            <div className="card-body">
              {data.addresses.length === 0 ? (
                <p className="text-muted">No address saved. Please add one in your profile.</p>
              ) : data.addresses.map(a => (
                <label key={a.id}
                  className={`d-block p-3 border rounded mb-2 cursor-pointer ${selectedAddress === a.id ? 'border-primary bg-light' : ''}`}
                  style={{ cursor: 'pointer' }}>
                  <input type="radio" name="address" value={a.id} checked={selectedAddress === a.id}
                    onChange={() => setSelectedAddress(a.id)} className="me-2" />
                  <strong>{a.fullName}</strong> · {a.phone}
                  <br />
                  <small className="text-muted">{a.addressLine}, {a.ward}, {a.district}, {a.province}</small>
                </label>
              ))}
            </div>
          </div>

          <div className="card shadow-sm mb-3">
            <div className="card-header"><h5 className="mb-0">Payment Method</h5></div>
            <div className="card-body d-flex gap-4">
              <div className="form-check">
                <input className="form-check-input" type="radio" name="payment" id="cod"
                  checked={paymentMethod === 'cod'} onChange={() => setPaymentMethod('cod')} />
                <label className="form-check-label" htmlFor="cod">Cash on Delivery</label>
              </div>
              <div className="form-check">
                <input className="form-check-input" type="radio" name="payment" id="payos"
                  checked={paymentMethod === 'payos'} onChange={() => setPaymentMethod('payos')} />
                <label className="form-check-label" htmlFor="payos">PayOS (Online)</label>
              </div>
            </div>
          </div>

          <div className="card shadow-sm mb-3">
            <div className="card-header"><h5 className="mb-0">Voucher</h5></div>
            <div className="card-body">
              <div className="input-group">
                <input className="form-control" placeholder="Enter voucher code" value={voucherCode}
                  onChange={e => setVoucherCode(e.target.value)} />
                <button className="btn btn-outline-secondary" onClick={handleApplyVoucher}>Apply</button>
              </div>
              {voucherMsg && (
                <small className={`mt-1 d-block ${voucherOk ? 'text-success' : 'text-danger'}`}>{voucherMsg}</small>
              )}
            </div>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="card shadow-sm sticky-top" style={{ top: 80 }}>
            <div className="card-header"><h5 className="mb-0">Order Summary</h5></div>
            <div className="card-body">
              {data.items.map((item, i) => (
                <div key={i} className="d-flex justify-content-between small mb-2">
                  <span>{item.productName} × {item.quantity}</span>
                  <span>{(item.price * item.quantity).toLocaleString()} ₫</span>
                </div>
              ))}
              <hr />
              {discount > 0 && (
                <div className="d-flex justify-content-between text-success small mb-2">
                  <span>Discount</span><span>-{discount.toLocaleString()} ₫</span>
                </div>
              )}
              <div className="d-flex justify-content-between fw-bold fs-5">
                <span>Total</span><span className="text-danger">{total.toLocaleString()} ₫</span>
              </div>
            </div>
            <div className="card-footer">
              {error && <div className="alert alert-danger py-2 mb-2">{error}</div>}
              <button className="btn btn-success w-100 btn-lg" onClick={handlePlaceOrder} disabled={placing}>
                {placing ? <span className="spinner-border spinner-border-sm me-2" /> : null}
                Place Order
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
