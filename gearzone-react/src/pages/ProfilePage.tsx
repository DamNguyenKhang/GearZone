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
    Promise.all([
      usersApi.getOrders(),
      usersApi.getAddresses(),
    ]).then(([o, a]) => {
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
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteAddress = async (id: string) => {
    await usersApi.deleteAddress(id);
    setAddresses(a => a.filter(x => x.id !== id));
  };

  const statusColors: Record<string, string> = {
    Pending: '#ed8936', Processing: '#3182ce', Shipping: '#805ad5',
    Delivered: '#38a169', Completed: '#38a169', Cancelled: '#e53e3e',
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 800, margin: '0 auto' }}>
      <h1>My Profile</h1>
      <p style={{ color: '#555' }}>{user?.fullName} · {user?.email}</p>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem' }}>
        <button onClick={() => setTab('orders')} style={{ fontWeight: tab === 'orders' ? 'bold' : 'normal', padding: '0.5rem 1rem' }}>My Orders</button>
        <button onClick={() => setTab('addresses')} style={{ fontWeight: tab === 'addresses' ? 'bold' : 'normal', padding: '0.5rem 1rem' }}>Addresses</button>
      </div>

      {tab === 'orders' && (
        <>
          {orders.length === 0 ? <p>No orders yet.</p> : orders.map(order => (
            <div key={order.id} style={{ padding: '1rem', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <p style={{ margin: 0, fontWeight: 600 }}>Order #{order.id.slice(0, 8)}</p>
                  <p style={{ margin: '0.25rem 0', color: '#888', fontSize: 13 }}>{new Date(order.createdAt).toLocaleDateString()}</p>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <p style={{ margin: 0, color: statusColors[order.status] ?? '#333', fontWeight: 600 }}>{order.status}</p>
                  <p style={{ margin: 0, fontWeight: 700 }}>{order.totalPrice?.toLocaleString()} VND</p>
                </div>
              </div>
              {order.subOrders && order.subOrders.length > 0 && (
                <div style={{ marginTop: '0.5rem', display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                  {order.subOrders.map(sub => (
                    <Link key={sub.id} to={`/orders/track/${sub.id}`}
                      style={{ fontSize: 12, padding: '0.2rem 0.6rem', background: '#eee', borderRadius: 4, textDecoration: 'none', color: '#333' }}>
                      Track #{sub.id.slice(0, 8)}
                    </Link>
                  ))}
                </div>
              )}
            </div>
          ))}
        </>
      )}

      {tab === 'addresses' && (
        <>
          {addresses.map(a => (
            <div key={a.id} style={{ padding: '1rem', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '0.75rem', display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div>
                <p style={{ margin: 0, fontWeight: 600 }}>{a.fullName} {a.isDefault && <span style={{ background: '#bee3f8', color: '#2b6cb0', fontSize: 11, padding: '1px 6px', borderRadius: 4, marginLeft: 6 }}>Default</span>}</p>
                <p style={{ margin: '0.2rem 0', color: '#666', fontSize: 14 }}>{a.phone}</p>
                <p style={{ margin: 0, color: '#555', fontSize: 14 }}>{a.addressLine}, {a.ward}, {a.district}, {a.province}</p>
              </div>
              <button onClick={() => handleDeleteAddress(a.id)}
                style={{ background: 'none', border: '1px solid #fed7d7', borderRadius: 4, color: '#c53030', cursor: 'pointer', padding: '0.25rem 0.75rem' }}>
                Delete
              </button>
            </div>
          ))}

          <button onClick={() => setShowAddAddress(!showAddAddress)}
            style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', marginTop: '0.5rem' }}>
            + Add Address
          </button>

          {showAddAddress && (
            <form onSubmit={handleAddAddress} style={{ marginTop: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', maxWidth: 500 }}>
              {(['fullName', 'phone', 'addressLine', 'ward', 'district', 'province'] as const).map(field => (
                <input key={field} placeholder={field.charAt(0).toUpperCase() + field.slice(1).replace(/([A-Z])/g, ' $1')}
                  value={addrForm[field]} onChange={e => setAddrForm(f => ({ ...f, [field]: e.target.value }))}
                  required style={{ padding: '0.5rem' }} />
              ))}
              <button type="submit" disabled={saving} style={{ padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                {saving ? 'Saving…' : 'Save Address'}
              </button>
            </form>
          )}
        </>
      )}
    </div>
  );
}
