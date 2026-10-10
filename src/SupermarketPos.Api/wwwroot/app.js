* { box-sizing: border-box; }
body {
  margin: 0;
  font-family: Segoe UI, Tahoma, sans-serif;
  background: #f3f6fb;
  color: #1d2433;
}

.hidden { display: none; }

.login-box {
  max-width: 420px;
  margin: 100px auto;
  padding: 24px;
  border-radius: 16px;
  background: white;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
}

.login-box h2 { margin-top: 0; }

.login-box input,
.login-box button,
.toolbar input,
.cart-meta input,
.cart-meta select {
  width: 100%;
  margin-bottom: 12px;
  padding: 10px 12px;
  border-radius: 10px;
  border: 1px solid #ccd7ec;
  font-size: 1rem;
}

.login-box button, .primary-btn, .tab, .product-item button, .cart-action, #logout-btn {
  cursor: pointer;
}

#login-btn, .primary-btn {
  background: #1d4ed8;
  color: white;
  border: none;
  font-weight: 600;
}

#app {
  max-width: 1400px;
  margin: 0 auto;
  padding: 18px;
}

.topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 16px 18px;
  background: #ffffff;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
}

.topbar h1 { margin: 0; font-size: 2rem; }

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

#logout-btn {
  border: none;
  background: #e5e7eb;
  padding: 8px 12px;
  border-radius: 8px;
}

.tabbar {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
}

.tab {
  border: none;
  background: #e8edf8;
  color: #2d3d5c;
  padding: 10px 18px;
  border-radius: 10px;
  font-weight: 600;
}

.tab.active {
  background: #1d4ed8;
  color: white;
}

.tab-panel { display: none; }
.tab-panel.active { display: block; }

.panel-row, .supplier-layout {
  display: grid;
  grid-template-columns: 1.3fr 1fr;
  gap: 18px;
}

.panel {
  background: #fff;
  border-radius: 14px;
  padding: 18px;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
}

.product-list,
.cart-items,
.inventory-list,
.summary-list,
.supplier-list,
.purchase-order-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.product-item,
.cart-item,
.inventory-item,
.summary-item,
.supplier-item,
.purchase-order-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border: 1px solid #e5ebf5;
  background: #f9fbff;
  border-radius: 10px;
}

.product-item h4,
.cart-item h4,
.inventory-item h4,
.summary-item h4,
.supplier-item h4,
.purchase-order-item h4 {
  margin: 0 0 4px;
}

.product-item small,
.cart-item small,
.inventory-item small,
.summary-item small,
.supplier-item small,
.purchase-order-item small {
  color: #596882;
}

.product-item button,
.cart-action {
  background: #22c55e;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 12px;
  font-weight: 600;
}

.cart-action {
  background: #ef4444;
}

.totals {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.totals div {
  display: flex;
  justify-content: space-between;
  font-size: 1.1rem;
}

.totals .grand {
  font-size: 1.3rem;
  border-top: 1px solid #e5ebf5;
  padding-top: 12px;
}

.primary-btn {
  width: 100%;
  margin-top: 12px;
  padding: 12px;
  font-size: 1.05rem;
}

.receipt-output {
  margin-top: 16px;
  background: #f9fafb;
  border: 1px solid #dfe8f5;
  border-radius: 10px;
  padding: 12px;
  min-height: 60px;
  white-space: pre-wrap;
  font-size: 0.94rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(180px, 1fr));
  gap: 18px;
  margin-bottom: 18px;
}

.metric-box {
  display: flex;
  flex-direction: column;
  justify-content: center;
  background: #fff;
  border-radius: 14px;
  padding: 18px;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
}

.metric-box label {
  color: #596882;
  margin-bottom: 8px;
}

.metric-box strong {
  font-size: 2rem;
}

.low-stock {
  color: #dc2626;
  font-weight: 700;
}

.hint {
  color: #596882;
  font-size: 0.85rem;
  text-align: center;
}

.badge {
  background: #e0f2fe;
  color: #0f172a;
  border-radius: 999px;
  padding: 6px 10px;
  font-size: 0.75rem;
  font-weight: 700;
}

@media (max-width: 900px) {
  .panel-row, .dashboard-grid, .supplier-layout { grid-template-columns: 1fr; }
}
