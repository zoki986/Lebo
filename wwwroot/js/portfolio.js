/**
 * Portfolio Manager - New API-based approach with partial view rendering
 * Makes AJAX calls to /api/portfolio/images and appends HTML to portfolioGrid
 */
class PortfolioManager {
    constructor() {
        this.currentPage = 1;
        this.currentCategory = 'all';
        this.isLoading = false;
        this.hasMoreImages = true;
        this.apiBaseUrl = '/api/portfolio';
        
        // DOM elements
        this.portfolioGrid = document.getElementById('portfolioGrid');
        this.loadingIndicator = document.getElementById('loadingIndicator');
        this.filterButtons = document.querySelectorAll('.category-btn');
        
        // Validate required elements
        if (!this.portfolioGrid) {
            console.error('Portfolio grid element not found');
            return;
        }
        
        this.init();
    }
    
    async init() {
        console.log('🚀 Initializing new Portfolio API system...');
        
        try {
            // Load initial images
            await this.loadInitialImages();
            
            // Setup event listeners
            this.setupFilterButtons();
            this.setupScrollListener();
            
            console.log('✅ Portfolio system initialized successfully');
        } catch (error) {
            console.error('❌ Failed to initialize portfolio system:', error);
        }
    }
    
    async loadInitialImages() {
        try {
            this.showLoading();
            console.log('📥 Loading initial images...');
            
            const html = await this.fetchPortfolioHTML(1, 8, this.currentCategory);
            
            if (html) {
                this.portfolioGrid.innerHTML = html;
                this.updatePaginationState();
                this.currentPage = 2; // Next page to load
                console.log('✅ Initial images loaded successfully');
            } else {
                console.warn('⚠️ No initial images received');
                this.portfolioGrid.innerHTML = '<div class="portfolio-no-results"><p>No images found.</p></div>';
            }
        } catch (error) {
            console.error('❌ Failed to load initial images:', error);
            this.portfolioGrid.innerHTML = '<div class="portfolio-error"><p>Failed to load images. Please try again.</p></div>';
        } finally {
            this.hideLoading();
        }
    }
    
    async loadMoreImages() {
        if (this.isLoading || !this.hasMoreImages) {
            console.log(`⏭️ Skipping load more: isLoading=${this.isLoading}, hasMoreImages=${this.hasMoreImages}`);
            return;
        }
        
        try {
            this.isLoading = true;
            this.showLoading();
            
            console.log(`📥 Loading page ${this.currentPage} for category: ${this.currentCategory}`);
            
            const html = await this.fetchPortfolioHTML(this.currentPage, 8, this.currentCategory);
            
            if (html && html.trim()) {
                // Append new images to existing grid
                this.portfolioGrid.insertAdjacentHTML('beforeend', html);
                this.updatePaginationState();
                this.currentPage++;

                // Refresh lightbox to include new images
                if (window.portfolioLightbox) {
                    window.portfolioLightbox.refreshImages();
                }

                console.log(`✅ Page ${this.currentPage - 1} loaded successfully`);
            } else {
                this.hasMoreImages = false;
                console.log('🏁 No more images available');
            }
        } catch (error) {
            console.error('❌ Failed to load more images:', error);
        } finally {
            this.isLoading = false;
            this.hideLoading();
        }
    }
    
    async loadCategoryImages(category) {
        try {
            this.showLoading();
            console.log(`🔍 Loading category: ${category}`);
            
            this.currentCategory = category;
            this.currentPage = 1;
            this.hasMoreImages = true;
            
            const html = await this.fetchPortfolioHTML(1, 8, category);
            
            if (html) {
                this.portfolioGrid.innerHTML = html;
                this.updatePaginationState();
                this.currentPage = 2;

                // Refresh lightbox to include new images
                if (window.portfolioLightbox) {
                    window.portfolioLightbox.refreshImages();
                }

                console.log(`✅ Category ${category} loaded successfully`);
            } else {
                this.portfolioGrid.innerHTML = '<div class="portfolio-no-results"><p>No images found for this category.</p></div>';
                this.hasMoreImages = false;
            }
        } catch (error) {
            console.error(`❌ Failed to load category ${category}:`, error);
            this.portfolioGrid.innerHTML = '<div class="portfolio-error"><p>Failed to load images. Please try again.</p></div>';
        } finally {
            this.hideLoading();
        }
    }
    
    async fetchPortfolioHTML(page, pageSize, category) {
        const url = `${this.apiBaseUrl}/images?page=${page}&pageSize=${pageSize}&category=${encodeURIComponent(category)}`;
        
        console.log(`🌐 Fetching: ${url}`);
        
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Accept': 'text/html',
                'Cache-Control': 'public, max-age=1800'
            }
        });
        
        if (!response.ok) {
            if (response.status === 404) {
                console.log('📭 No more images found (404)');
                return null;
            }
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const html = await response.text();
        console.log(`📦 Received ${html.length} characters of HTML`);
        
        return html;
    }
    
    updatePaginationState() {
        // Look for pagination data in the loaded HTML
        const paginationData = this.portfolioGrid.querySelector('.portfolio-pagination-data');
        if (paginationData) {
            this.hasMoreImages = paginationData.dataset.hasNext === 'true';
            console.log(`📊 Pagination updated: hasMore=${this.hasMoreImages}`);
        }
    }
    
    setupFilterButtons() {
        this.filterButtons.forEach(button => {
            button.addEventListener('click', async (e) => {
                e.preventDefault();
                
                const newCategory = button.dataset.filter;
                if (newCategory === this.currentCategory) return;
                
                // Update active button
                this.filterButtons.forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');
                
                // Load new category
                await this.loadCategoryImages(newCategory);
            });
        });
    }
    
    setupScrollListener() {
        let scrollTimeout;
        
        window.addEventListener('scroll', () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                if (this.shouldLoadMore()) {
                    this.loadMoreImages();
                }
            }, 100);
        }, { passive: true });
    }
    
    shouldLoadMore() {
        if (this.isLoading || !this.hasMoreImages) return false;
        
        const scrollPosition = window.innerHeight + window.scrollY;
        const documentHeight = document.documentElement.offsetHeight;
        const threshold = 300; // Load when 300px from bottom
        
        return scrollPosition >= documentHeight - threshold;
    }
    
    showLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.classList.add('show');
        }
    }
    
    hideLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.classList.remove('show');
        }
    }
}

// Export for global use
window.PortfolioManager = PortfolioManager;

// Auto-initialize if DOM is already loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.portfolioManager = new PortfolioManager();
    });
} else {
    window.portfolioManager = new PortfolioManager();
}
