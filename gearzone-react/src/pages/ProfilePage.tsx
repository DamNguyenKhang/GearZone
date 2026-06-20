import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { usersApi } from '../api/users';
import { useAuth } from '../contexts/AuthContext';

interface Order {
  id: string;
  createdAt: string;
  status: string;
  totalPrice: number;
  subOrders?: Array<{ id: string; status: string }>;
}

interface Address {
  id: string;
  fullName: string;
  phone: string;
  addressLine: string;
  province: string;
  district: string;
  ward: string;
  isDefault: boolean;
}

const statusBadge: Record<string, string> = {
  Pending: 'warning', Processing: 'primary', Shipping: 'info',
  Delivered: 'success', Completed: 'success', Cancelled: 'danger',
};

export default function ProfilePage() {
  const { user } = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [addresses, setAddresses] = useState<Address[]>([]);
  const [tab, setTab] = useState<'orders' | 'addresses'>('orders');
  const [loading, setLoading] = useState(true);
  const [showAddAddress, setShowAddAddress] = useState(false);
  const [addrForm, setAddrForm] = useState({ fullName: '', phone: '', addressLine: '', province: '', district: '', ward: '' });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    Promise.all([usersApi.getOrders(), usersApi.getAddresses()]).then(([o, a]) => {
      setOrders((o as { items?: Order[] }).items ?? (o as Order[]) ?? []);
      setAddresses(a as Address[] ?? []);
    }).finally(() => setLoading(false));
  }, []);

  const handleAddAddress = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      await usersApi.addAddress(addrForm);
      const updated = await usersApi.getAddresses();
      setAddresses(updated as Address[]);
      setShowAddAddress(false);
      setAddrForm({ fullName: '', phone: '', addressLine: '', province: '', district: '', ward: '' });
    } finally { setSaving(false); }
  };

  const handleDeleteAddress = async (id: string) => {
    await usersApi.deleteAddress(id);
    setAddresses(a => a.filter(x => x.id !== id));
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <div className="d-flex align-items-center gap-3 mb-4">
        <div className="rounded-circle bg-primary text-white d-flex align-items-center justify-content-center fw-bold fs-4"
          style={{ width: 56, height: 56 }}>
          {user?.fullName?.[0]}
        </div>
        <div>
          <h5 className="mb-0">{user?.fullName}</h5>
          <small className="text-muted">{user?.email}</small>
        </div>
      </div>

      <ul className="nav nav-tabs mb-4">
        <li className="nav-item">
          <button className={`nav-link ${tab === 'orders' ? 'active' : ''}`} onClick={() => setTab('orders')}>My Orders</button>
        </li>
        <li className="nav-item">
          <button className={`nav-link ${tab === 'addresses' ? 'active' : ''}`} onClick={() => setTab('addresses')}>Addresses</button>
        </li>
      </ul>

      {tab === 'orders' && (
        orders.length === 0 ? <p className="text-muted">No orders yet.</p> : (
          orders.map(order => (
            <div key={order.id} className="card mb-3 shadow-sm">
              <div className="card-body d-flex justify-content-between align-items-center">
                <div>
                  <p className="fw-semibold mb-0">Order #{order.id.slice(0, 8)}</p>
                  <small className="text-muted">{new Date(order.createdAt).toLocaleDateString()}</small>
                  {order.subOrders && order.subOrders.length > 0 && (
                    <div className="mt-2 d-flex gap-2 flex-wrap">
                      {order.subOrders.map(sub => (
                        <Link key={sub.id} to={`/orders/track/${sub.id}`}
                          className="badge bg-secondary text-decoration-none">
                          Track #{sub.id.slice(0, 8)}
                        </Link>
                      ))}
                    </div>
                  )}
                </div>
                <div className="text-end">
                  <span className={`badge bg-${statusBadge[order.status] ?? 'secondary'} mb-1`}>{order.status}</span>
                  <p className="fw-bold mb-0">{order.totalPrice?.toLocaleString()} ₫</p>
                </div>
              </div>
            </div>
          ))
        )
      )}

      {tab === 'addresses' && (
        <>
          {addresses.map(a => (
            <div key={a.id} className="card mb-2 shadow-sm">
              <div className="card-body d-flex justify-content-between align-items-start">
                <div>
                  <p className="fw-semibold mb-0">
                    {a.fullName}
                    {a.isDefault && <span className="badge bg-primary ms-2 fw-normal">Default</span>}
                  </p>
                  <small className="text-muted d-block">{a.phone}</small>
                  <small className="text-muted">{a.addressLine}, {a.ward}, {a.district}, {a.province}</small>
                </div>
                <button className="btn btn-outline-danger btn-sm" onClick={() => handleDeleteAddress(a.id)}>Delete</button>
              </div>
            </div>
          ))}

          <button className="btn btn-primary mt-2" onClick={() => setShowAddAddress(!showAddAddress)}>
            + Add Address
          </button>

          {showAddAddress && (
            <div className="card mt-3 shadow-sm">
              <div className="card-body">
                <h6 className="card-title">New Address</h6>
                <form onSubmit={handleAddAddress}>
                  <div className="row g-2">
                    {(['fullName', 'phone', 'addressLine', 'ward', 'district', 'province'] as const).map(field => (
                      <div key={field} className="col-md-6">
                        <input className="form-control"
                          placeholder={field.charAt(0).toUpperCase() + field.slice(1).replace(/([A-Z])/g, ' $1')}
                          value={addrForm[field]}
                          onChange={e => setAddrForm(f => ({ ...f, [field]: e.target.value }))}
                          required />
                      </div>
                    ))}
                  </div>
                  <div className="d-flex gap-2 mt-3">
                    <button type="submit" className="btn btn-success" disabled={saving}>
                      {saving ? <span className="spinner-border spinner-border-sm me-1" /> : null}Save
                    </button>
                    <button type="button" className="btn btn-outline-secondary" onClick={() => setShowAddAddress(false)}>Cancel</button>
                  </div>
                </form>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
