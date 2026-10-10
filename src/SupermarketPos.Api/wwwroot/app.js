const state = {
  token: localStorage.getItem('posToken') || '',
  user: JSON.parse(localStorage.getItem('posUser') || 'null'),
  products: [],
  inventory: [],
  cart: [],
  summary: null,
  suppliers: [],
  poList: []
};

const loginBox = document.getElementById('login-box');
const app = document.getElementById('app');
const userLabel = document.getElementById('user-label');

const tabs = document.querySelectorAll('.tab');
const tabPanels = {
  foh: document.getElementById('foh-tab'),
  boh: document.getElementById('boh-tab'),
  suppliers: document.getElementById('suppliers-tab'),
  products: document.getElementById('products-tab')
};

function setActiveTab(tabName) {
  tabs.forEach(tab => tab.classList.toggle('active', tab.dataset.tab === tabName));
  Object.entries(tabPanels).forEach(([name, panel]) => {
    panel.classList.toggle('active', name === tabName);
  });
}

tabs.forEach(tab => tab.addEventListener('click', () => setActiveTab(tab.dataset.tab)));

async function apiRequest(path, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {})
  };

  if (state.token) {
    headers.Authorization = `Bearer ${state.token}`;
  }

  const response = await fetch(path, {
    ...options,
    headers
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || 'Request failed');
  }

  if (response.status === 204) return null;
  return response.json();
}

function showLoggedInState() {
  if (!state.token) {
    loginBox.classList.remove('hidden');
    app.classList.add('hidden');
    return;
  }

  loginBox.classList.add('hidden');
  app.classList.remove('hidden');
  userLabel.textContent = state.user ? state.user.fullName || state.user.username : 'User';
}

async function loadDashboard() {
  const [products, inventory, summary] = await Promise.all([
    apiRequest('/api/products'),
    apiRequest('/api/inventory'),
    apiRequest('/api/dashboard')
  ]);

  state.products = products;
  state.inventory = inventory;
  renderProducts();
  renderInventory();
  renderSummary(summary);
  renderProductManagementList();
}

async function loadSuppliers() {
  try {
    const data = await apiRequest('/api/suppliers');
    state.suppliers = data;
    renderSuppliers();
  } catch {
    document.getElementById('supplier-list').innerHTML = '<div class="hint">Supplier API not available yet.</div>';
  }
}

async function loadPurchaseOrders() {
  try {
    const data = await apiRequest('/api/purchase-orders');
    state.poList = data;
    renderPurchaseOrders();
  } catch {
    document.getElementById('purchase-order-list').innerHTML = '<div class="hint">Purchase-order API not available yet.</div>';
  }
}

async function login() {
  const username = document.getElementById('username').value;
  const password = document.getElementById('password').value;

  const result = await apiRequest('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password })
  });

  state.token = result.token;
  state.user = result.user;
  localStorage.setItem('posToken', result.token);
  localStorage.setItem('posUser', JSON.stringify(result.user));
  showLoggedInState();
  await Promise.all([loadDashboard(), loadSuppliers(), loadPurchaseOrders()]);
}

function logout() {
  state.token = '';
  state.user = null;
  localStorage.removeItem('posToken');
  localStorage.removeItem('posUser');
  showLoggedInState();
}

document.getElementById('login-btn').addEventListener('click', login);
document.getElementById('logout-btn').addEventListener('click', logout);

function renderProducts() {
  const list = document.getElementById('product-list');
  const search = document.getElementById('product-search').value.toLowerCase();
  const visible = state.products.filter(product => {
    const text = `${product.name} ${product.sku}`.toLowerCase();
    return text.includes(search);
  });

  list.innerHTML = visible.map(product => `
    <div class="product-item">
      <div>
        <h4>${product.name}</h4>
        <small>${product.category} • ${product.sku}</small>
      </div>
      <div>
        <div><strong>$${Number(product.sellingPrice).toFixed(2)}</strong></div>
        <small>Qty: ${product.quantity}</small>
      </div>
      <button data-add="${product.id}">Add</button>
    </div>
  `).join('') || '<div class="hint">No products found.</div>';

  list.querySelectorAll('[data-add]').forEach(button => {
    button.addEventListener('click', () => addToCart(Number(button.dataset.add)));
  });
}

function addToCart(productId) {
  const product = state.products.find(item => item.id === productId);
  if (!product) return;

  const existing = state.cart.find(item => item.id === productId);
  if (existing) {
    existing.quantity += 1;
  } else {
    state.cart.push({ ...product, quantity: 1 });
  }

  updateCart();
}

function updateCart() {
  const cartItems = document.getElementById('cart-items');
  const subtotalEl = document.getElementById('subtotal');
  const taxEl = document.getElementById('tax');
  const totalEl = document.getElementById('total');

  if (!state.cart.length) {
    cartItems.innerHTML = '<div class="hint">Cart is empty.</div>';
    subtotalEl.textContent = '0.00';
    taxEl.textContent = '0.00';
    totalEl.textContent = '0.00';
    return;
  }

  let subtotal = 0;
  cartItems.innerHTML = state.cart.map(item => {
    subtotal += Number(item.sellingPrice) * item.quantity;
    return `
      <div class="cart-item">
        <div>
          <h4>${item.name}</h4>
          <small>${item.quantity} × $${Number(item.sellingPrice).toFixed(2)}</small>
        </div>
        <button class="cart-action" data-remove="${item.id}">Remove</button>
      </div>
    `;
  }).join('');

  const tax = subtotal * 0.08;
  const total = subtotal + tax;
  subtotalEl.textContent = subtotal.toFixed(2);
  taxEl.textContent = tax.toFixed(2);
  totalEl.textContent = total.toFixed(2);

  document.querySelectorAll('[data-remove]').forEach(button => {
    button.addEventListener('click', () => {
      const id = Number(button.dataset.remove);
      state.cart = state.cart.filter(item => item.id !== id);
      updateCart();
    });
  });
}

function renderInventory() {
  const list = document.getElementById('inventory-list');
  list.innerHTML = state.inventory.map(item => `
    <div class="inventory-item ${item.isLowStock ? 'low-stock' : ''}">
      <div>
        <h4>${item.name}</h4>
        <small>${item.quantityOnHand} units on hand</small>
      </div>
      <span class="badge">${item.isLowStock ? 'Low stock' : 'Healthy'}</span>
    </div>
  `).join('') || '<div class="hint">No inventory data.</div>';
}

function renderSummary(summary) {
  const list = document.getElementById('summary-list');
  if (!summary) {
    list.innerHTML = '<div class="hint">Summary unavailable.</div>';
    return;
  }

  document.getElementById('metric-products').textContent = summary.totalProducts ?? 0;
  document.getElementById('metric-low-stock').textContent = summary.lowStockItems ?? 0;
  document.getElementById('metric-stock').textContent = summary.totalStock ?? 0;
  document.getElementById('metric-sales').textContent = `$${Number(summary.weeklySales || 0).toFixed(2)}`;

  list.innerHTML = (summary.topCategories || []).map(category => `
    <div class="summary-item">
      <div>
        <h4>${category.category}</h4>
      </div>
      <strong>$${Number(category.total).toFixed(2)}</strong>
    </div>
  `).join('') || '<div class="hint">No category data.</div>';
}

function renderSuppliers() {
  const list = document.getElementById('supplier-list');
  list.innerHTML = state.suppliers.map(supplier => `
    <div class="supplier-item">
      <div>
        <h4>${supplier.name}</h4>
        <small>${supplier.contactPerson} • ${supplier.phone}</small>
      </div>
      <span class="badge">${supplier.creditAvailable}</span>
    </div>
  `).join('') || '<div class="hint">No suppliers found.</div>';
}

function renderPurchaseOrders() {
  const list = document.getElementById('purchase-order-list');
  list.innerHTML = state.poList.map(order => `
    <div class="purchase-order-item">
      <div>
        <h4>${order.poNumber}</h4>
        <small>${order.name} • ${order.status}</small>
      </div>
      <strong>$${Number(order.totalAmount || 0).toFixed(2)}</strong>
    </div>
  `).join('') || '<div class="hint">No purchase orders.</div>';
}

async function refreshProductManagementList() {
  try {
    const data = await apiRequest('/api/product-management/products');
    state.products = data;
    renderProductManagementList();
  } catch (error) {
    document.getElementById('product-management-list').innerHTML = `<div class="hint">${error.message}</div>`;
  }
}

function renderProductManagementList() {
  const list = document.getElementById('product-management-list');
  list.innerHTML = state.products.map(product => `
    <div class="product-management-item">
      <div>
        <h4>${product.name}</h4>
        <small>${product.sku} • ${product.category}</small>
      </div>
      <div>
        <div><strong>$${Number(product.sellingPrice).toFixed(2)}</strong></div>
        <small>Stock: ${product.quantity}</small>
      </div>
      <div>
        <button data-edit="${product.id}">Edit</button>
      </div>
    </div>
  `).join('') || '<div class="hint">No products in catalog.</div>';

  list.querySelectorAll('[data-edit]').forEach(button => {
    button.addEventListener('click', () => populateProductForm(Number(button.dataset.edit)));
  });
}

function populateProductForm(id) {
  const product = state.products.find(item => item.id === id);
  if (!product) return;

  document.getElementById('product-id').value = product.id;
  document.getElementById('product-name').value = product.name;
  document.getElementById('product-sku').value = product.sku;
  document.getElementById('product-category').value = product.category;
  document.getElementById('product-cost-price').value = product.costPrice;
  document.getElementById('product-selling-price').value = product.sellingPrice;
  document.getElementById('product-reorder-level').value = product.reorderLevel;
  document.getElementById('product-quantity').value = product.quantity;
  document.getElementById('product-active').checked = product.isActive;

  setActiveTab('products');
}

async function handleProductSubmit(event) {
  event.preventDefault();

  const id = Number(document.getElementById('product-id').value || 0);
  const payload = {
    name: document.getElementById('product-name').value,
    sku: document.getElementById('product-sku').value,
    category: document.getElementById('product-category').value,
    costPrice: Number(document.getElementById('product-cost-price').value),
    sellingPrice: Number(document.getElementById('product-selling-price').value),
    reorderLevel: Number(document.getElementById('product-reorder-level').value),
    quantityOnHand: Number(document.getElementById('product-quantity').value),
    isActive: document.getElementById('product-active').checked
  };

  try {
    if (id) {
      await apiRequest(`/api/product-management/products/${id}`, {
        method: 'PUT',
        body: JSON.stringify(payload)
      });
    } else {
      await apiRequest('/api/product-management/products', {
        method: 'POST',
        body: JSON.stringify(payload)
      });
    }

    resetProductForm();
    await refreshProductManagementList();
    await loadDashboard();
  } catch (error) {
    alert(error.message);
  }
}

async function deleteProduct(id) {
  try {
    await apiRequest(`/api/product-management/products/${id}`, { method: 'DELETE' });
    resetProductForm();
    await refreshProductManagementList();
    await loadDashboard();
  } catch (error) {
    alert(error.message);
  }
}

function resetProductForm() {
  document.getElementById('product-form').reset();
  document.getElementById('product-id').value = '';
  document.getElementById('product-active').checked = true;
}

document.getElementById('product-search').addEventListener('input', renderProducts);
document.getElementById('checkout-btn').addEventListener('click', async () => {
  if (!state.cart.length) {
    alert('Add items to the cart before checkout.');
    return;
  }

  const customerName = document.getElementById('customer-name').value;
  const paymentMethod = document.getElementById('payment-method').value;

  try {
    const result = await apiRequest('/api/sales/checkout', {
      method: 'POST',
      body: JSON.stringify({
        customerName,
        paymentMethod,
        items: state.cart.map(item => ({ productId: item.id, quantity: item.quantity }))
      })
    });

    document.getElementById('receipt-output').textContent = `Receipt: ${result.receiptNumber}\nSubtotal: $${result.subtotal.toFixed(2)}\nTax: $${result.tax.toFixed(2)}\nTotal: $${result.total.toFixed(2)}`;
    state.cart = [];
    updateCart();
    await loadDashboard();
  } catch (error) {
    alert(error.message);
  }
});

document.getElementById('product-form').addEventListener('submit', handleProductSubmit);
document.getElementById('product-reset').addEventListener('click', resetProductForm);

showLoggedInState();
if (state.token) {
  (async () => {
    await Promise.all([loadDashboard(), loadSuppliers(), loadPurchaseOrders(), refreshProductManagementList()]);
    setActiveTab('foh');
  })();
} else {
  setActiveTab('foh');
}

window.addEventListener('DOMContentLoaded', () => {
  setActiveTab('foh');
});

window.addEventListener('keydown', (event) => {
  if (event.key === 'Escape') {
    resetProductForm();
  }
});









































































































































































provider
