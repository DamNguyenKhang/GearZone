import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { reviewsApi } from '../api/reviews';

interface ReviewEditor {
  orderItemId: string;
  productName: string;
  productImageUrl?: string;
  existingRating?: number;
  existingComment?: string;
}

export default function WriteReviewPage() {
  const { orderItemId } = useParams<{ orderItemId: string }>();
  const navigate = useNavigate();
  const [editor, setEditor] = useState<ReviewEditor | null>(null);
  const [loading, setLoading] = useState(true);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!orderItemId) return;
    reviewsApi.getEditor(orderItemId)
      .then(d => {
        const data = d as ReviewEditor;
        setEditor(data);
        if (data.existingRating) setRating(data.existingRating);
        if (data.existingComment) setComment(data.existingComment);
      })
      .finally(() => setLoading(false));
  }, [orderItemId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!orderItemId) return;
    setSubmitting(true); setError('');
    try {
      await reviewsApi.submit({ orderItemId, rating, comment });
      navigate('/profile');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to submit review.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!editor) return <div style={{ padding: '2rem' }}>Order item not found.</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 500, margin: '2rem auto' }}>
      <h1>Write a Review</h1>

      <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', marginBottom: '1.5rem', padding: '1rem', background: '#f9f9f9', borderRadius: 8 }}>
        {editor.productImageUrl && (
          <img src={editor.productImageUrl} alt={editor.productName} style={{ width: 64, height: 64, objectFit: 'cover', borderRadius: 4 }} />
        )}
        <p style={{ margin: 0, fontWeight: 600 }}>{editor.productName}</p>
      </div>

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Rating</label>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            {[1, 2, 3, 4, 5].map(n => (
              <button key={n} type="button" onClick={() => setRating(n)}
                style={{ fontSize: 28, background: 'none', border: 'none', cursor: 'pointer', color: n <= rating ? '#f6ad55' : '#ccc' }}>
                ★
              </button>
            ))}
          </div>
          <p style={{ margin: '0.25rem 0 0', color: '#666', fontSize: 13 }}>{rating} / 5</p>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 600 }}>Comment</label>
          <textarea value={comment} onChange={e => setComment(e.target.value)}
            placeholder="Share your experience with this product…"
            rows={5} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', borderRadius: 4, border: '1px solid #ccc' }} />
        </div>

        {error && <p style={{ color: 'red' }}>{error}</p>}

        <button type="submit" disabled={submitting}
          style={{ padding: '0.75rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          {submitting ? 'Submitting…' : 'Submit Review'}
        </button>
      </form>
    </div>
  );
}
