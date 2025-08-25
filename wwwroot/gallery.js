// Gallery API - Server-side approach using Umbraco.Media.GetAll()
// Works with image data embedded directly in HTML from server-side Razor

class ServerSideGalleryAPI {
    constructor() {
        // Get images from server-generated data
        this.allImages = window.portfolioImages || [];

        // Cache for loaded images to avoid duplicates
        this.imageCache = new Map();

        console.log(`Server-side Gallery API initialized with ${this.allImages.length} images from Umbraco Media`);

        // Log category breakdown
        const fashionCount = this.allImages.filter(img => img.category === 'fashion-portraits').length;
        const foodCount = this.allImages.filter(img => img.category === 'food-beverage').length;
        console.log(`Fashion & Portraits: ${fashionCount} images`);
        console.log(`Food & Beverage: ${foodCount} images`);
    }


    // Main API method: GET /api/gallery/images
    async getImages(page = 1, limit = 8, category = 'all') {
        try {
            let filteredImages = this.allImages;

            // Filter by category if specified
            if (category !== 'all') {
                filteredImages = this.allImages.filter(img => img.category === category);
            }

            // Calculate pagination
            const startIndex = (page - 1) * limit;
            const endIndex = startIndex + limit;
            const paginatedImages = filteredImages.slice(startIndex, endIndex);

            console.log(`Serving page ${page} of ${category} category: ${paginatedImages.length} images`);

            return {
                success: true,
                data: {
                    images: paginatedImages,
                    pagination: {
                        page: page,
                        limit: limit,
                        total: filteredImages.length,
                        totalPages: Math.ceil(filteredImages.length / limit),
                        hasMore: endIndex < filteredImages.length
                    },
                    category: category
                },
                timestamp: new Date().toISOString()
            };
        } catch (error) {
            console.error('Error in getImages:', error);
            return {
                success: false,
                data: {
                    images: [],
                    pagination: {
                        page: page,
                        limit: limit,
                        total: 0,
                        totalPages: 0,
                        hasMore: false
                    },
                    category: category
                },
                error: error.message,
                timestamp: new Date().toISOString()
            };
        }
    }

    // API endpoint: GET /api/gallery/categories
    async getCategories() {
        try {
            const fashionCount = this.allImages.filter(img => img.category === 'fashion-portraits').length;
            const foodCount = this.allImages.filter(img => img.category === 'food-beverage').length;

            return {
                success: true,
                data: [
                    { id: 'all', name: 'All', count: this.allImages.length },
                    { id: 'fashion-portraits', name: 'Fashion & Portraits', count: fashionCount },
                    { id: 'food-beverage', name: 'Food & Beverage', count: foodCount }
                ]
            };
        } catch (error) {
            console.error('Error in getCategories:', error);
            return {
                success: true,
                data: [
                    { id: 'all', name: 'All', count: 0 },
                    { id: 'fashion-portraits', name: 'Fashion & Portraits', count: 0 },
                    { id: 'food-beverage', name: 'Food & Beverage', count: 0 }
                ]
            };
        }
    }

    // Method to get initial images (first 4)
    async getInitialImages() {
        return await this.getImages(1, 4, 'all');
    }

    // Method to load more images
    async loadMoreImages(page, category = 'all') {
        return await this.getImages(page, 8, category);
    }

    // Method to preload images for better performance
    preloadImages(images, callback) {
        let loadedCount = 0;
        const totalImages = images.length;

        if (totalImages === 0) {
            if (callback) callback();
            return;
        }

        images.forEach(imageData => {
            const img = new Image();

            img.onload = () => {
                loadedCount++;
                console.log(`Preloaded image: ${imageData.title} (${loadedCount}/${totalImages})`);

                if (loadedCount === totalImages && callback) {
                    callback();
                }
            };

            img.onerror = () => {
                loadedCount++;
                console.warn(`Failed to preload image: ${imageData.title}`);

                if (loadedCount === totalImages && callback) {
                    callback();
                }
            };

            img.src = imageData.url;
        });
    }

    // Method to get image statistics
    getImageStats() {
        const stats = {
            total: this.allImages.length,
            categories: {},
            avgTitleLength: 0,
            hasOriginalUrls: 0,
            hasCroppedUrls: 0
        };

        this.allImages.forEach(img => {
            // Count by category
            if (!stats.categories[img.category]) {
                stats.categories[img.category] = 0;
            }
            stats.categories[img.category]++;

            // Calculate average title length
            stats.avgTitleLength += img.title.length;

            // Check URL types
            if (img.originalUrl && img.originalUrl !== img.url) {
                stats.hasCroppedUrls++;
            }
            if (img.originalUrl) {
                stats.hasOriginalUrls++;
            }
        });

        stats.avgTitleLength = Math.round(stats.avgTitleLength / this.allImages.length);

        return stats;
    }

    // Method to search images by title or alt text
    async searchImages(query, category = 'all') {
        let filteredImages = this.allImages;

        // Filter by category first
        if (category !== 'all') {
            filteredImages = filteredImages.filter(img => img.category === category);
        }

        // Then filter by search query
        if (query && query.trim()) {
            const searchTerm = query.toLowerCase().trim();
            filteredImages = filteredImages.filter(img =>
                img.title.toLowerCase().includes(searchTerm) ||
                img.alt.toLowerCase().includes(searchTerm)
            );
        }

        return {
            success: true,
            data: {
                images: filteredImages,
                query: query,
                category: category,
                total: filteredImages.length
            }
        };
    }
}

// Create global instance - this replaces the API-based gallery
window.galleryAPI = new ServerSideGalleryAPI();

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ServerSideGalleryAPI;
}

// Log statistics on load
window.addEventListener('load', () => {
    if (window.galleryAPI) {
        const stats = window.galleryAPI.getImageStats();
        console.log('📊 Image Statistics:', stats);
    }
});
