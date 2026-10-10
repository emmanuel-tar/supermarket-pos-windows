const state = {
  products: [],
  cart: [],
  inventory: [],
  token: sessionStorage.getItem('posToken') || '',
  user: JSON.parse(sessionStorage.getItem('posUser') || 'null')
};

const loginBox = document.getElementById('login-box');
const appView = document.getElementById('app');
const userLabel = document.getElementById('user-label');

function toggleAuthView() {
  if (state.token) {
    loginBox.classList.add('hidden');
    appView.classList.remove('hidden');
    userLabel.textContent = state.user ? `${state.user.fullName} (${state.user.role})` : 'User';
    initApp();
  } else {
    loginBox.classList.remove('hidden');
    appView.classList.add('hidden');
  }
}

async function login() {
  const username = document.getElementById('username').value;
  const password = document.getElementById('password').value;

  const res = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });

  if (!res.ok) {
    alert('Invalid username or password');
    return;
  }

  const data = await res.json();
  state.token = data.token;
  state.user = data.user;
  sessionStorage.setItem('posToken', state.token);
  sessionStorage.setItem('posUser', JSON.stringify(state.user));
  toggleAuthView();
}

function logout() {
  state.token = '';
  state.user = null;
  sessionStorage.removeItem('posToken');
  sessionStorage.removeItem('posUser');
  toggleAuthView();
}

function getAuthHeaders() {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${state.token}`
  };
}

async function fetchJson(url, options = {}) {
  const response = await fetch(url, { ...options, headers: { ...getAuthHeaders(), ...(options.headers || {}) } });
  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }
  return await response.json();
}

async function loadProducts() {
  const data = await fetchJson('/api/products');
  state.products = data;
  renderProducts();
}

async function loadInventory() {
  const data = await fetchJson('/api/inventory');
  state.inventory = data;
  renderInventory();
}

async function loadDashboard() {
  const data = await fetchJson('/api/dashboard');
  document.getElementById('metric-products').textContent = data.totalProducts;
  document.getElementById('metric-low-stock').textContent = data.lowStockItems;
  document.getElementById('metric-stock').textContent = data.totalStock;
  document.getElementById('metric-sales').textContent = '$' + Number(data.weeklySales).toFixed(2);

  const summary = await fetchJson('/api/reports/summary');
  const summaryList = document.getElementById('summary-list');
  summaryList.innerHTML = `
    <div class="summary-item"><div><h4>Cash Sales</h4><small>Last 24h</small></div><strong>$${Number(summary.cashSales).toFixed(2)}</strong></div>
    <div class="summary-item"><div><h4>Card Sales</h4><small>Last 24h</small></div><strong>$${Number(summary.cardSales).toFixed(2)}</strong></div>
    <div class="summary-item"><div><h4>Net Sales</h4><small>After returns</small></div><strong>$${Number(summary.netSales).toFixed(2)}</strong></div>
    <div class="summary-item"><div><h4>Alerts</h4><small>Daily checks</small></div><strong>${summary.alerts.length}</strong></div>
  `;
}

function renderProducts() {
  const query = document.getElementById('product-search').value.toLowerCase();
  const filtered = state.products.filter(product =>
    product.name.toLowerCase().includes(query) ||
    product.sku.toLowerCase().includes(query) ||
    product.category.toLowerCase().includes(query)
  );

  document.getElementById('product-list').innerHTML = filtered.map(product => `
    <div class="product-item">
      <div>
        <h4>${product.name}</h4>
        <small>${product.category} • ${product.quantity} in stock</small>
      </div>
      <div>
        <strong>$${Number(product.sellingPrice).toFixed(2)}</strong>
        <div><button data-id="${product.id}">Add</button></div>
      </div>
    </div>
  `).join('');

  document.querySelectorAll('#product-list button').forEach(button => {
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
    document.getElementById('cart-items').innerHTML = '<div class="cart-item"><div><h4>No items</h4><small>Add products to begin checkout</small></div></div>';
    document.getElementById('subtotal').textContent = '0.00';
    document.getElementById('tax').textContent = '0.00';
    document.getElementById('total').textContent = '0.00';
    return;
  }

  let subtotal = 0;
  const cartItems = document.getElementById('cart-items');

  cartItems.innerHTML = state.cart.map(item => {
    const lineTotal = item.product.sellingPrice * item.quantity;
    subtotal += lineTotal;

    return `
      <div class="cart-item">
        <div>
          <h4>${item.product.name}</h4>
          <small>$${Number(item.product.sellingPrice).toFixed(2)} each</small>
        </div>
        <div>
          <div>
            <button class="cart-action" data-action="minus" data-id="${item.productId}">-</button>
            <span>${item.quantity}</span>
            <button class="cart-action" data-action="plus" data-id="${item.productId}">+</button>
          </div>
          <strong>$${Number(lineTotal).toFixed(2)}</strong>
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

  document.getElementById('subtotal').textContent = subtotal.toFixed(2);
  document.getElementById('tax').textContent = tax.toFixed(2);
  document.getElementById('total').textContent = total.toFixed(2);
}

function renderInventory() {
  const inventoryList = document.getElementById('inventory-list');
  inventoryList.innerHTML = state.inventory.map(item => `
    <div class="inventory-item">
      <div>
        <h4>${item.name}</h4>
        <small>Unit price: $${Number(item.sellingPrice).toFixed(2)}</small>
      </div>
      <strong class="${item.isLowStock ? 'low-stock' : ''}">${item.quantityOnHand}</strong>
    </div>
  `).join('');
}

async function checkout() {
  if (state.cart.length === 0) {
    document.getElementById('receipt-output').textContent = 'Cart is empty.';
    return;
  }

  const payload = {
    customerName: document.getElementById('customer-name').value || 'Walk-in Customer',
    paymentMethod: document.getElementById('payment-method').value,
    items: state.cart.map(item => ({ productId: item.productId, quantity: item.quantity }))
  };

  try {
    const result = await fetchJson('/api/sales/checkout', {
      method: 'POST',
      body: JSON.stringify(payload)
    });

    document.getElementById('receipt-output').textContent = `Receipt: ${result.receiptNumber}\nCustomer: ${payload.customerName}\nPayment: ${payload.paymentMethod}\nSubtotal: $${Number(result.subtotal).toFixed(2)}\nTax: $${Number(result.tax).toFixed(2)}\nTotal: $${Number(result.total).toFixed(2)}`;
    state.cart = [];
    renderCart();
    await loadProducts();
    await loadInventory();
    await loadDashboard();
  } catch (err) {
    alert('Checkout failed. Please check stock and try again.');
  }
}

document.getElementById('login-btn').addEventListener('click', login);
document.getElementById('logout-btn').addEventListener('click', logout);
document.getElementById('product-search').addEventListener('input', renderProducts);
document.getElementById('checkout-btn').addEventListener('click', checkout);

async function initApp() {
  await loadProducts();
  await loadInventory();
  await loadDashboard();
  renderCart();
}

toggleAuthView();
