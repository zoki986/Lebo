import {
  LitElement,
  html,
  css,
  customElement,
  state,
} from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import {
  UmbNotificationContext,
  UMB_NOTIFICATION_CONTEXT,
} from "@umbraco-cms/backoffice/notification";

interface ContactMessage {
  id: string;
  name: string;
  email: string;
  message: string;
  submittedAt: string;
}

interface PagedResult {
  items?: ContactMessage[];
  totalItems?: number;
}

@customElement("contact-messages-dashboard")
export class ContactMessagesDashboardElement extends UmbElementMixin(LitElement) {
  #notificationContext?: UmbNotificationContext;

  @state()
  private page = 1;

  @state()
  private pageSize = 10;

  @state()
  private pageSizes = [10, 25, 50, 100];

  @state()
  private totalItems = 0;

  @state()
  private totalPages = 1;

  @state()
  private messages: ContactMessage[] = [];

  @state()
  private loading = false;

  // guards
  private inflight: string | null = null;
  private lastLoadedKey: string | null = null;

  constructor() {
    super();
    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
      this.#notificationContext = instance;
    });
  }

  connectedCallback() {
    super.connectedCallback();
    this.load();
  }

  private async load() {
    const key = `p:${this.page}|s:${this.pageSize}`;
    if (this.inflight === key || this.lastLoadedKey === key) return;

    this.inflight = key;
    this.loading = true;

    try {
      const response = await fetch(
        `/umbraco/backoffice/contact?page=${this.page}&pageSize=${this.pageSize}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: PagedResult = await response.json();

      this.messages = data.items || [];
      this.totalItems = data.totalItems || 0;
      this.totalPages = Math.max(1, Math.ceil(this.totalItems / this.pageSize));
      this.lastLoadedKey = key;
    } catch (error) {
      this.messages = [];
      this.totalItems = 0;
      this.totalPages = 1;

      this.#notificationContext?.peek("danger", {
        data: {
          headline: "Load failed",
          message: error instanceof Error ? error.message : "Unknown error",
        },
      });
    } finally {
      this.inflight = null;
      this.loading = false;
    }
  }

  private changePageSize(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.pageSize = parseInt(target.value, 10);
    this.page = 1;
    this.lastLoadedKey = null;
    this.load();
  }

  private onPageChange(newPage: number) {
    const page = Math.max(1, Math.min(this.totalPages, newPage));
    if (page === this.page) return;
    
    this.page = page;
    this.load();
  }

  private formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString();
  }

  private truncateMessage(message: string, maxLength = 120): string {
    if (message.length <= maxLength) return message;
    return message.substring(0, maxLength) + "…";
  }

  render() {
      return html`
      <uui-box>
        <div slot="headline">
          <h1>Contact Messages</h1>
        </div>

        <div class="controls">
          <label>
            Page size:
            <select @change=${this.changePageSize} .value=${this.pageSize.toString()}>
              ${this.pageSizes.map(
          (size) => html`<option value=${size}>${size}</option>`
      )}
            </select>
          </label>
          <span>${this.totalItems} items</span>
        </div>

        ${this.loading
              ? html`<uui-loader></uui-loader>`
              : html`
              <uui-table>
                <uui-table-head>
                  <uui-table-head-cell>Name</uui-table-head-cell>
                  <uui-table-head-cell>Email</uui-table-head-cell>
                  <uui-table-head-cell>Message</uui-table-head-cell>
                  <uui-table-head-cell>Submitted</uui-table-head-cell>
                </uui-table-head>
                ${this.messages.length === 0
                      ? html`
                      <uui-table-row>
                        <uui-table-cell colspan="4">
                          <em>No messages found.</em>
                        </uui-table-cell>
                      </uui-table-row>
                    `
                      : this.messages.map(
                          (msg) =>   html`
                        <uui-table-row>
                          <uui-table-cell>${msg.name}</uui-table-cell>
                          <uui-table-cell>${msg.email}</uui-table-cell>
                          <uui-table-cell title=${msg.message}>
                            ${this.truncateMessage(msg.message)}
                          </uui-table-cell>
                          <uui-table-cell>
                            ${this.formatDate(msg.submittedAt)}
                          </uui-table-cell>
                        </uui-table-row>
                      `
                    )}
              </uui-table>

              ${this.totalPages > 1
                ? html`
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
                  `
                : ""}
            `}
      </uui-box>
    `;
  }

  static styles = css`
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
  `;
}

export default ContactMessagesDashboardElement;

declare global {
  interface HTMLElementTagNameMap {
    "contact-messages-dashboard": ContactMessagesDashboardElement;
  }
}
