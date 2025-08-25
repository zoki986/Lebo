import { LitElement as b, html as n, css as f, state as u, customElement as v } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as $ } from "@umbraco-cms/backoffice/element-api";
import { UMB_NOTIFICATION_CONTEXT as y } from "@umbraco-cms/backoffice/notification";
var g = Object.defineProperty, _ = Object.getOwnPropertyDescriptor, d = (e) => {
  throw TypeError(e);
}, P = (e, t, a) => t in e ? g(e, t, { enumerable: !0, configurable: !0, writable: !0, value: a }) : e[t] = a, o = (e, t, a, l) => {
  for (var r = l > 1 ? void 0 : l ? _(t, a) : t, c = e.length - 1, p; c >= 0; c--)
    (p = e[c]) && (r = (l ? p(t, a, r) : p(r)) || r);
  return l && r && g(t, a, r), r;
}, s = (e, t, a) => P(e, typeof t != "symbol" ? t + "" : t, a), m = (e, t, a) => t.has(e) || d("Cannot " + a), z = (e, t, a) => (m(e, t, "read from private field"), t.get(e)), S = (e, t, a) => t.has(e) ? d("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), C = (e, t, a, l) => (m(e, t, "write to private field"), t.set(e, a), a), h;
let i = class extends $(b) {
  constructor() {
    super(), S(this, h), s(this, "page", 1), s(this, "pageSize", 10), s(this, "pageSizes", [10, 25, 50, 100]), s(this, "totalItems", 0), s(this, "totalPages", 1), s(this, "messages", []), s(this, "loading", !1), s(this, "inflight", null), s(this, "lastLoadedKey", null), this.consumeContext(y, (e) => {
      C(this, h, e);
    });
  }
  connectedCallback() {
    super.connectedCallback(), this.load();
  }
  async load() {
    var t;
    const e = `p:${this.page}|s:${this.pageSize}`;
    if (!(this.inflight === e || this.lastLoadedKey === e)) {
      this.inflight = e, this.loading = !0;
      try {
        const a = await fetch(
          `/umbraco/backoffice/contact?page=${this.page}&pageSize=${this.pageSize}`,
          {
            method: "GET",
            headers: {
              "Content-Type": "application/json"
            }
          }
        );
        if (!a.ok)
          throw new Error(`HTTP error! status: ${a.status}`);
        const l = await a.json();
        this.messages = l.items || [], this.totalItems = l.totalItems || 0, this.totalPages = Math.max(1, Math.ceil(this.totalItems / this.pageSize)), this.lastLoadedKey = e;
      } catch (a) {
        this.messages = [], this.totalItems = 0, this.totalPages = 1, (t = z(this, h)) == null || t.peek("danger", {
          data: {
            headline: "Load failed",
            message: a instanceof Error ? a.message : "Unknown error"
          }
        });
      } finally {
        this.inflight = null, this.loading = !1;
      }
    }
  }
  changePageSize(e) {
    const t = e.target;
    this.pageSize = parseInt(t.value, 10), this.page = 1, this.lastLoadedKey = null, this.load();
  }
  onPageChange(e) {
    const t = Math.max(1, Math.min(this.totalPages, e));
    t !== this.page && (this.page = t, this.load());
  }
  formatDate(e) {
    return new Date(e).toLocaleString();
  }
  truncateMessage(e, t = 120) {
    return e.length <= t ? e : e.substring(0, t) + "…";
  }
  render() {
    return n`
      <uui-box>
        <div slot="headline">
          <h1>Contact Messages</h1>
        </div>

        <div class="controls">
          <label>
            Page size:
            <select @change=${this.changePageSize} .value=${this.pageSize.toString()}>
              ${this.pageSizes.map(
      (e) => n`<option value=${e}>${e}</option>`
    )}
            </select>
          </label>
          <span>${this.totalItems} items</span>
        </div>

        ${this.loading ? n`<uui-loader></uui-loader>` : n`
              <uui-table>
                <uui-table-head>
                  <uui-table-head-cell>Name</uui-table-head-cell>
                  <uui-table-head-cell>Email</uui-table-head-cell>
                  <uui-table-head-cell>Message</uui-table-head-cell>
                  <uui-table-head-cell>Submitted</uui-table-head-cell>
                </uui-table-head>
                ${this.messages.length === 0 ? n`
                      <uui-table-row>
                        <uui-table-cell colspan="4">
                          <em>No messages found.</em>
                        </uui-table-cell>
                      </uui-table-row>
                    ` : this.messages.map(
      (e) => n`
                        <uui-table-row>
                          <uui-table-cell>${e.name}</uui-table-cell>
                          <uui-table-cell>${e.email}</uui-table-cell>
                          <uui-table-cell title=${e.message}>
                            ${this.truncateMessage(e.message)}
                          </uui-table-cell>
                          <uui-table-cell>
                            ${this.formatDate(e.submittedAt)}
                          </uui-table-cell>
                        </uui-table-row>
                      `
    )}
              </uui-table>

              ${this.totalPages > 1 ? n`
                    <div class="pagination">
                      <uui-button
                        @click=${() => this.onPageChange(this.page - 1)}
                        ?disabled=${this.page <= 1}
                        compact
                      >
                        Previous
                      </uui-button>
                      <span>Page ${this.page} of ${this.totalPages}</span>
                      <uui-button
                        @click=${() => this.onPageChange(this.page + 1)}
                        ?disabled=${this.page >= this.totalPages}
                        compact
                      >
                        Next
                      </uui-button>
                    </div>
                  ` : ""}
            `}
      </uui-box>
    `;
  }
};
h = /* @__PURE__ */ new WeakMap();
s(i, "styles", f`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .controls {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-space-4);
    }

    .controls label {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .controls select {
      padding: var(--uui-size-space-2);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: var(--uui-size-space-4);
      margin-top: var(--uui-size-space-4);
    }

    uui-loader {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-space-6);
    }
  `);
o([
  u()
], i.prototype, "page", 2);
o([
  u()
], i.prototype, "pageSize", 2);
o([
  u()
], i.prototype, "pageSizes", 2);
o([
  u()
], i.prototype, "totalItems", 2);
o([
  u()
], i.prototype, "totalPages", 2);
o([
  u()
], i.prototype, "messages", 2);
o([
  u()
], i.prototype, "loading", 2);
i = o([
  v("contact-messages-dashboard")
], i);
const E = i;
export {
  i as ContactMessagesDashboardElement,
  E as default
};
//# sourceMappingURL=contact-messages-dashboard.js.map
