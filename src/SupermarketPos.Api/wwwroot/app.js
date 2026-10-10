const state = {
  products: [],
  cart: [],
  inventory: []
};

const productList = document.getElementById('product-list');
const cartItems = document.getElementById('cart-items');
const subtotalEl = document.getElementById('subtotal');
const taxEl = document.getElementById('tax');
const totalEl = document.getElementById('total');
const receiptOutput = document.getElementById('receipt-output');
const paymentMethod = document.getElementById('payment-method');
const customerName = document.getElementById('customer-name');

async function loadProducts() {
  const res = await fetch('/api/products');
  state.products = await res.json();
  renderProducts();
}

async function loadInventory() {
  const res = await fetch('/api/inventory');
  state.inventory = await res.json();
  renderInventory();
}

async function loadDashboard() {
  const res = await fetch('/api/dashboard');
  const data = await res.json();
  document.getElementById('metric-products').textContent = data.totalProducts;
  document.getElementById('metric-low-stock').textContent = data.lowStockItems;
  document.getElementById('metric-stock').textContent = data.totalStock;
  document.getElementById('metric-sales').textContent = '$' + data.weeklySales.toFixed(2);

  const summaryList = document.getElementById('summary-list');
  const summaryRes = await fetch('/api/reports/summary');
  const summary = await summaryRes.json();

  summaryList.innerHTML = `
    <div class="summary-item"><div><h4>Cash Sales</h4><small>Current period</small></div><strong>$${summary.cashSales.toFixed(2)}</strong></div>
    <div class="summary-item"><div><h4>Card Sales</h4><small>Current period</small></div><strong>$${summary.cardSales.toFixed(2)}</strong></div>
    <div class="summary-item"><div><h4>Net Sales</h4><small>After returns</small></div><strong>$${summary.netSales.toFixed(2)}</strong></div>
    <div class="summary-item"><div><h4>Alerts</h4><small>Daily checks</small></div><strong>${summary.alerts.length}</strong></div>
  `;
}

function renderProducts() {
  const search = document.getElementById('product-search').value.toLowerCase();

  const filtered = state.products.filter(product =>
    product.name.toLowerCase().includes(search) ||
    product.sku.toLowerCase().includes(search) ||
    product.category.toLowerCase().includes(search)
  );

  productList.innerHTML = filtered.map(product => `
    <div class="product-item">
      <div>
        <h4>${product.name}</h4>
        <small>${product.category} • ${product.quantity} in stock</small>
      </div>
      <div>
        <strong>$${product.sellingPrice.toFixed(2)}</strong>
        <div><button data-id="${product.id}">Add</button></div>
      </div>
    </div>
  `).join('');

  productList.querySelectorAll('button').forEach(button => {
    button.addEventListener('click', () => addToCart(Number(button.dataset.id)));
  });
}

function addToCart(productId) {
  const existing = state.cart.find(item => item.productId === productId);
  if (existing) {
    existing.quantity += 1;
  } else {
    const product = state.products.find(p => p.id === productId);
    state.cart.push({ productId, quantity: 1, product });
  }
  renderCart();
}

function changeCartQuantity(productId, delta) {
  const item = state.cart.find(i => i.productId === productId);
  if (!item) return;
  item.quantity += delta;
  if (item.quantity <= 0) {
    state.cart = state.cart.filter(i => i.productId !== productId);
  }
  renderCart();
}

function renderCart() {
  if (state.cart.length === 0) {
    cartItems.innerHTML = '<div class="cart-item"><div><h4>No items</h4><small>Add products to begin checkout</small></div></div>';
    subtotalEl.textContent = '0.00';
    taxEl.textContent = '0.00';
    totalEl.textContent = '0.00';
    return;
  }

  let subtotal = 0;

  cartItems.innerHTML = state.cart.map(item => {
    const lineTotal = item.product.sellingPrice * item.quantity;
    subtotal += lineTotal;
    return `
      <div class="cart-item">
        <div>
          <h4>${item.product.name}</h4>
          <small>$${item.product.sellingPrice.toFixed(2)} each</small>
        </div>
        <div>
          <div class="qty-controls">
            <button class="cart-action" data-action="minus" data-id="${item.productId}">-</button>
            <span>${item.quantity}</span>
            <button class="cart-action" data-action="plus" data-id="${item.productId}">+</button>
          </div>
          <strong>$${lineTotal.toFixed(2)}</strong>
        </div>
      </div>
    `;
  }).join('');

  cartItems.querySelectorAll('.cart-action').forEach(button => {
    button.addEventListener('click', () => {
      const productId = Number(button.dataset.id);
      const action = button.dataset.action;
      changeCartQuantity(productId, action === 'plus' ? 1 : -1);
    });
  });

  const tax = subtotal * 0.08;
  const total = subtotal + tax;

  subtotalEl.textContent = subtotal.toFixed(2);
  taxEl.textContent = tax.toFixed(2);
  totalEl.textContent = total.toFixed(2);
}

function renderInventory() {
  const inventoryList = document.getElementById('inventory-list');

  inventoryList.innerHTML = state.inventory.map(item => `
    <div class="inventory-item">
      <div>
        <h4>${item.name}</h4>
        <small>Unit price: $${item.sellingPrice.toFixed(2)}</small>
      </div>
      <strong class="${item.isLowStock ? 'low-stock' : ''}">${item.quantity}</strong>
    </div>
  `).join('');
}

async function checkout() {
  if (state.cart.length === 0) {
    receiptOutput.textContent = 'Cart is empty.';
    return;
  }

  const payload = {
    customerName: customerName.value || 'Walk-in Customer',
    paymentMethod: paymentMethod.value,
    items: state.cart.map(item => ({
      productId: item.productId,
      quantity: item.quantity
    }))
  };

  const response = await fetch('/api/sales/checkout', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(payload)
  });

  const result = await response.json();

  if (!response.ok) {
    receiptOutput.textContent = result.errors.join('\n');
    return;
  }

  receiptOutput.textContent = `Receipt: ${result.receiptNumber}\nCustomer: ${payload.customerName}\nPayment: ${payload.paymentMethod}\nSubtotal: $${result.subtotal.toFixed(2)}\nTax: $${(result.total - result.subtotal).toFixed(2)}\nTotal: $${result.total.toFixed(2)}`;
  state.cart = [];
  renderCart();
  await loadProducts();
  await loadInventory();
  await loadDashboard();
}

document.getElementById('product-search').addEventListener('input', renderProducts);
document.getElementById('checkout-btn').addEventListener('click', checkout);

for (const tab of document.querySelectorAll('.tab')) {
  tab.addEventListener('click', () => {
    document.querySelectorAll('.tab').forEach(t => t.classList.toggle('active', t === tab));
    const target = tab.dataset.tab;
    document.querySelectorAll('.tab-panel').forEach(panel => {
      panel.classList.toggle('active', panel.id === `${target}-tab`);
    });
  });
}

(async function init() {
  await loadProducts();
  await loadInventory();
  await loadDashboard();
  renderCart();
})();

window.addEventListener('DOMContentLoaded', () => {
  renderCart();
});
