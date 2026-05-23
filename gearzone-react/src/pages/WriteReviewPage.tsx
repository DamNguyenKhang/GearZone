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
    reviewsApi.getEditor(orderItemId).then(d => {
      const data = d as ReviewEditor;
      setEditor(data);
      if (data.existingRating) setRating(data.existingRating);
      if (data.existingComment) setComment(data.existingComment);
    }).finally(() => setLoading(false));
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
    } finally { setSubmitting(false); }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;
  if (!editor) return <div className="container py-4"><div className="alert alert-warning">Order item not found.</div></div>;

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <h2 className="mb-4">Write a Review</h2>

          <div className="card shadow-sm mb-4">
            <div className="card-body d-flex align-items-center gap-3">
              {editor.productImageUrl && (
                <img src={editor.productImageUrl} alt={editor.productName}
                  className="rounded" style={{ width: 64, height: 64, objectFit: 'cover' }} />
              )}
              <p className="fw-semibold mb-0">{editor.productName}</p>
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label fw-semibold">Rating</label>
              <div className="d-flex gap-1">
                {[1, 2, 3, 4, 5].map(n => (
                  <button key={n} type="button" onClick={() => setRating(n)}
                    className="btn p-0 border-0" style={{ fontSize: 32, color: n <= rating ? '#f6ad55' : '#ccc' }}>
                    ★
                  </button>
                ))}
              </div>
              <small className="text-muted">{rating} / 5</small>
            </div>

            <div className="mb-3">
              <label className="form-label fw-semibold">Comment</label>
              <textarea className="form-control" rows={5} value={comment}
                onChange={e => setComment(e.target.value)}
                placeholder="Share your experience with this product…" />
            </div>

            {error && <div className="alert alert-danger py-2">{error}</div>}

            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? <span className="spinner-border spinner-border-sm me-2" /> : null}
              Submit Review
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
