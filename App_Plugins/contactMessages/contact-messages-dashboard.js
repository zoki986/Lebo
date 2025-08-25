// Simple JavaScript version for immediate testing
class ContactMessagesDashboard extends HTMLElement {
  constructor() {
    super();
    this.page = 1;
    this.pageSize = 10;
    this.pageSizes = [10, 25, 50, 100];
    this.totalItems = 0;
    this.totalPages = 1;
    this.messages = [];
    this.loading = false;
    this.inflight = null;
    this.lastLoadedKey = null;
  }

  connectedCallback() {
    this.render();
    this.load();
  }

  async load() {
    const key = `p:${this.page}|s:${this.pageSize}`;
    if (this.inflight === key || this.lastLoadedKey === key) return;
    
    this.inflight = key;
    this.loading = true;
    this.render();

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
        const errorText = await response.text();
        throw new Error(`HTTP error! status: ${response.status}, body: ${errorText}`);
      }

      const data = await response.json();

      // Try multiple possible property names
      const items = data.items || data.Items || data.results || data.Results || data.data || data.Data || [];
      const totalItems = data.totalItems || data.TotalItems || data.total || data.Total || data.count || data.Count || 0;

      this.messages = Array.isArray(items) ? items : [];
      this.totalItems = totalItems;
      this.totalPages = Math.max(1, Math.ceil(this.totalItems / this.pageSize));
      this.lastLoadedKey = key;

    } catch (error) {
      console.error('Load failed:', error);
      this.messages = [];
      this.totalItems = 0;
      this.totalPages = 1;
      
      // Show error in UI
      this.showError(error.message);
    } finally {
      this.inflight = null;
      this.loading = false;
      this.render();
    }
  }

  showError(message) {
    // Simple error display
    const errorDiv = document.createElement('div');
    errorDiv.style.cssText = 'background: #f8d7da; color: #721c24; padding: 12px; border: 1px solid #f5c6cb; border-radius: 4px; margin: 10px 0;';
    errorDiv.textContent = `Error: ${message}`;

    // Remove any existing error
    const existingError = this.querySelector('.error-message');
    if (existingError) {
      existingError.remove();
    }

    errorDiv.className = 'error-message';
    this.appendChild(errorDiv);
  }

  showContactModal(contact) {
    // Create modal overlay with Umbraco-style backdrop
    const modal = document.createElement('div');
    modal.className = 'umb-overlay umb-overlay--show';
    modal.style.cssText = `
      position: fixed; top: 0; left: 0; width: 100%; height: 100%;
      background: rgba(0,0,0,0.4); z-index: 1000; display: flex;
      align-items: center; justify-content: center;
    `;

    // Format the submitted date
    const formattedDate = new Date(contact.submittedAt).toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });

    // Create modal content with Umbraco styling
    modal.innerHTML = `
      <div class="umb-modal" style="background: white; border-radius: 6px; box-shadow: 0 20px 60px rgba(0,0,0,0.15); max-width: 700px; width: 90%; max-height: 90vh; overflow: hidden; display: flex; flex-direction: column;">
        <!-- Header -->
        <div class="umb-modal-header" style="padding: 20px 24px; border-bottom: 1px solid #d8dde6; display: flex; justify-content: space-between; align-items: center; background: #f7f9fc;">
          <h3 style="margin: 0; color: #1b264f; font-size: 18px; font-weight: 600;">Contact Message Details</h3>
          <button onclick="this.closest('.umb-overlay').remove()" style="background: none; border: none; font-size: 20px; cursor: pointer; color: #8a9ba8; padding: 4px; line-height: 1;">&times;</button>
        </div>

        <!-- Content -->
        <div class="umb-modal-body" style="padding: 24px; overflow-y: auto; flex: 1;">
          <!-- Contact Info Grid -->
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 24px;">
            <div class="umb-property">
              <div class="umb-property-label" style="font-weight: 600; color: #1b264f; margin-bottom: 6px; font-size: 14px;">Full Name</div>
              <div class="umb-property-value" style="padding: 10px 12px; background: #f7f9fc; border: 1px solid #d8dde6; border-radius: 4px; color: #1b264f;">
                ${contact.name || 'Not provided'}
              </div>
            </div>

            <div class="umb-property">
              <div class="umb-property-label" style="font-weight: 600; color: #1b264f; margin-bottom: 6px; font-size: 14px;">Email Address</div>
              <div class="umb-property-value" style="padding: 10px 12px; background: #f7f9fc; border: 1px solid #d8dde6; border-radius: 4px; color: #1b264f;">
                <a href="mailto:${contact.email}" style="color: #3544b1; text-decoration: none;">${contact.email || 'Not provided'}</a>
              </div>
            </div>
          </div>

          <!-- Submission Details -->
          <div style="margin-bottom: 24px;">
            <div class="umb-property">
              <div class="umb-property-label" style="font-weight: 600; color: #1b264f; margin-bottom: 6px; font-size: 14px;">Submitted</div>
              <div class="umb-property-value" style="padding: 10px 12px; background: #f7f9fc; border: 1px solid #d8dde6; border-radius: 4px; color: #1b264f;">
                ${formattedDate}
              </div>
            </div>
          </div>

          <!-- Message Content -->
          <div class="umb-property">
            <div class="umb-property-label" style="font-weight: 600; color: #1b264f; margin-bottom: 6px; font-size: 14px;">Message</div>
            <div class="umb-property-value" style="padding: 16px; background: #f7f9fc; border: 1px solid #d8dde6; border-radius: 4px; color: #1b264f; white-space: pre-wrap; line-height: 1.6; min-height: 120px; max-height: 300px; overflow-y: auto;">
              ${contact.message || 'No message provided'}
            </div>
          </div>

          <!-- Contact ID (for reference) -->
          <div style="margin-top: 20px; padding-top: 20px; border-top: 1px solid #d8dde6;">
            <div class="umb-property">
              <div class="umb-property-label" style="font-weight: 600; color: #8a9ba8; margin-bottom: 6px; font-size: 12px;">Reference ID</div>
              <div class="umb-property-value" style="font-family: monospace; font-size: 11px; color: #8a9ba8;">
                ${contact.id}
              </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="umb-modal-footer" style="padding: 16px 24px; border-top: 1px solid #d8dde6; background: #f7f9fc; display: flex; justify-content: flex-end; gap: 12px;">
          <button onclick="navigator.clipboard.writeText('Name: ${contact.name}\\nEmail: ${contact.email}\\nMessage: ${contact.message}\\nSubmitted: ${formattedDate}'); this.textContent='Copied!'; setTimeout(() => this.textContent='Copy Details', 2000);"
                  style="padding: 8px 16px; background: #f2f5f8; color: #1b264f; border: 1px solid #d8dde6; border-radius: 4px; cursor: pointer; font-size: 14px;">
            Copy Details
          </button>
          <button onclick="this.closest('.umb-overlay').remove()"
                  style="padding: 8px 20px; background: #3544b1; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; font-weight: 500;">
            Close
          </button>
        </div>
      </div>
    `;

    // Close on overlay click (but not on modal content)
    modal.addEventListener('click', (e) => {
      if (e.target === modal) modal.remove();
    });

    // Close on Escape key
    const handleEscape = (e) => {
      if (e.key === 'Escape') {
        modal.remove();
        document.removeEventListener('keydown', handleEscape);
      }
    };
    document.addEventListener('keydown', handleEscape);

    document.body.appendChild(modal);
  }

  showContactByIndex(index) {
    if (this.messages && this.messages[index]) {
      this.showContactModal(this.messages[index]);
    }
  }

  changePageSize(newSize) {
    this.pageSize = parseInt(newSize, 10);
    this.page = 1;
    this.lastLoadedKey = null;
    this.load();
  }

  changePage(newPage) {
    const page = Math.max(1, Math.min(this.totalPages, newPage));
    if (page === this.page) return;
    
    this.page = page;
    this.load();
  }

  formatDate(dateString) {
    return new Date(dateString).toLocaleString();
  }

  truncateMessage(message, maxLength = 120) {
    if (message.length <= maxLength) return message;

    const truncated = message.substring(0, maxLength);
    return `${truncated}… <span style="color: #007cba; cursor: pointer; text-decoration: underline; font-size: 12px;" onclick="event.stopPropagation();">Read More</span>`;
  }

  toggleMessage(messageId) {
    const shortSpan = document.getElementById(messageId + '-short');
    const fullSpan = document.getElementById(messageId + '-full');

    if (shortSpan.style.display === 'none') {
      shortSpan.style.display = 'inline';
      fullSpan.style.display = 'none';
    } else {
      shortSpan.style.display = 'none';
      fullSpan.style.display = 'inline';
    }
  }

  render() {


    this.innerHTML = `
      <div style="padding: 20px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;">
        <h1>Contact Messages</h1>

        <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px;">
          <div style="display: flex; align-items: center; gap: 16px;">
            <label style="display: flex; align-items: center; gap: 8px;">
              Page size:
              <select onchange="this.closest('contact-messages-dashboard').changePageSize(this.value)" style="padding: 4px 8px; border: 1px solid #ccc; border-radius: 4px;">
                ${this.pageSizes.map(size =>
                  `<option value="${size}" ${size === this.pageSize ? 'selected' : ''}>${size}</option>`
                ).join('')}
              </select>
            </label>
            <span>${this.totalItems} items</span>
          </div>
          <div style="color: #666; font-size: 14px; font-style: italic;">
            💡 Click any row to view full details
          </div>
        </div>

        ${this.loading ? 
          '<div style="text-align: center; padding: 40px;">Loading...</div>' :
          `
            <table style="width: 100%; border-collapse: collapse; border: 1px solid #ddd;">
              <thead>
                <tr style="background: #f8f9fa;">
                  <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Name</th>
                  <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Email</th>
                  <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Message</th>
                  <th style="padding: 12px; text-align: left; border: 1px solid #ddd;">Submitted</th>
                </tr>
              </thead>
              <tbody>
                ${this.messages.length === 0 ? 
                  '<tr><td colspan="4" style="padding: 20px; text-align: center; font-style: italic;">No messages found.</td></tr>' :
                  this.messages.map((msg, index) => `
                    <tr data-contact-index="${index}"
                        onclick="this.closest('contact-messages-dashboard').showContactByIndex(${index})"
                        style="cursor: pointer; transition: background-color 0.2s;"
                        onmouseover="this.style.backgroundColor='#f8f9fa'"
                        onmouseout="this.style.backgroundColor=''"
                        title="Click to view full contact details">
                      <td style="padding: 12px; border: 1px solid #ddd;">${msg.name || ''}</td>
                      <td style="padding: 12px; border: 1px solid #ddd;">${msg.email || ''}</td>
                      <td style="padding: 12px; border: 1px solid #ddd;">
                        ${this.truncateMessage(msg.message || '')}
                      </td>
                      <td style="padding: 12px; border: 1px solid #ddd;">${this.formatDate(msg.submittedAt || '')}</td>
                    </tr>
                  `).join('')
                }
              </tbody>
            </table>

            ${this.totalPages > 1 ? `
              <div style="display: flex; align-items: center; justify-content: center; gap: 16px; margin-top: 16px;">
                <button 
                  onclick="this.closest('contact-messages-dashboard').changePage(${this.page - 1})"
                  ${this.page <= 1 ? 'disabled' : ''}
                  style="padding: 8px 16px; border: 1px solid #ccc; background: white; border-radius: 4px; cursor: pointer;"
                >
                  Previous
                </button>
                <span>Page ${this.page} of ${this.totalPages}</span>
                <button 
                  onclick="this.closest('contact-messages-dashboard').changePage(${this.page + 1})"
                  ${this.page >= this.totalPages ? 'disabled' : ''}
                  style="padding: 8px 16px; border: 1px solid #ccc; background: white; border-radius: 4px; cursor: pointer;"
                >
                  Next
                </button>
              </div>
            ` : ''}
          `
        }
      </div>
    `;
  }
}

// Register the custom element
customElements.define('contact-messages-dashboard', ContactMessagesDashboard);

export default ContactMessagesDashboard;
