// Professional Photography Studio - JavaScript
// This file is ready for future JavaScript functionality

// Example: Mobile menu toggle functionality
// function toggleMobileMenu() {
//   const navMenu = document.querySelector('.nav-menu');
//   navMenu.classList.toggle('mobile-active');
// }

// Portfolio Category Filtering
class PortfolioFilter {
  constructor() {
    this.filterButtons = document.querySelectorAll('.category-btn');
    this.portfolioItems = document.querySelectorAll('.portfolio-item');
    this.currentFilter = 'all';

    if (this.filterButtons.length > 0 && this.portfolioItems.length > 0) {
      this.init();
    }
  }

  init() {
    // Add click event listeners to filter buttons
    this.filterButtons.forEach(button => {
      button.addEventListener('click', (e) => {
        e.preventDefault();
        const filter = button.dataset.filter;
        this.filterPortfolio(filter);
        this.updateActiveButton(button);
      });
    });

    // Initialize all items as visible
    this.portfolioItems.forEach(item => {
      item.classList.add('filter-visible');
    });
  }

  filterPortfolio(category) {
    this.currentFilter = category;

    this.portfolioItems.forEach(item => {
      const itemCategory = item.dataset.category;

      if (category === 'all' || itemCategory === category) {
        // Show item with smooth transition
        item.classList.remove('filter-hidden');
        item.classList.add('filter-visible');
      } else {
        // Hide item with smooth transition
        item.classList.remove('filter-visible');
        item.classList.add('filter-hidden');
      }
    });
  }

  updateActiveButton(activeButton) {
    // Remove active class from all buttons
    this.filterButtons.forEach(button => {
      button.classList.remove('active');
    });

    // Add active class to clicked button
    activeButton.classList.add('active');
  }

  // Public method to get current filter
  getCurrentFilter() {
    return this.currentFilter;
  }

  // Public method to get visible items count
  getVisibleItemsCount() {
    return document.querySelectorAll('.portfolio-item.filter-visible').length;
  }
}

// Example: Contact form handling
// function handleContactForm(event) {
//   event.preventDefault();
//   // Add form submission logic here
//   console.log('Contact form submitted');
// }

// Continuous Left-to-Right Image Slider
// Creates a seamless continuous flow of images from left to right
class ContinuousSlider {
  constructor() {
    this.sliderTrack = document.getElementById('sliderTrack');

    if (this.sliderTrack) {
      this.init();
    }
  }

  init() {
    // Wait for images to load and layout to stabilize
    setTimeout(() => {
      this.setupContinuousLeftToRightSlider();
    }, 500);

    // Also recalculate on window resize
    window.addEventListener('resize', () => {
      setTimeout(() => {
        this.setupContinuousLeftToRightSlider();
      }, 100);
    });

    // Pause animation on hover for better user experience
    this.sliderTrack.addEventListener('mouseenter', () => {
      this.sliderTrack.style.animationPlayState = 'paused';
    });

    this.sliderTrack.addEventListener('mouseleave', () => {
      this.sliderTrack.style.animationPlayState = 'running';
    });
  }

  setupContinuousLeftToRightSlider() {
    // Clear any existing animation first
    this.sliderTrack.style.animation = 'none';

    const slides = this.sliderTrack.querySelectorAll('.slide');

    // Force layout recalculation
    this.sliderTrack.offsetHeight;

    // Calculate total width of all slides
    let totalWidth = 0;
    slides.forEach(slide => {
      const slideRect = slide.getBoundingClientRect();
      const slideStyle = window.getComputedStyle(slide);
      const marginRight = parseInt(slideStyle.marginRight) || 40;
      totalWidth += slideRect.width + marginRight;
    });

    // For continuous flow with duplicated content:
    // We have 2 sets of slides, so one set is totalWidth / 2
    const oneSetWidth = totalWidth / 2;

    // For seamless left-to-right continuous flow:
    // Start from -oneSetWidth (first set off-screen left)
    // End at 0 (second set moves to visible position)
    const startPos = -oneSetWidth;
    const endPos = 0;

    // Remove any existing custom styles
    const existingStyle = document.getElementById('marquee-custom-style');
    if (existingStyle) existingStyle.remove();

    // Create new keyframes for continuous left-to-right movement
    const keyframes = `
      @keyframes continuousLeftToRight {
        0% {
          transform: translateX(${startPos}px);
        }
        100% {
          transform: translateX(${endPos}px);
        }
      }
    `;

    const style = document.createElement('style');
    style.id = 'marquee-custom-style';
    style.textContent = keyframes;
    document.head.appendChild(style);

    // Calculate duration based on distance and speed
    const distance = Math.abs(endPos - startPos);
    const duration = distance / 80; // 80 pixels per second for smooth continuous flow

    // Apply animation with a slight delay to ensure styles are applied
    setTimeout(() => {
      this.sliderTrack.style.animation = `continuousLeftToRight ${duration}s linear infinite`;
    }, 10);
  }
}

// Back to Top Button functionality
class BackToTop {
  constructor() {
    this.button = document.querySelector('.back-to-top');
    if (this.button) {
      this.init();
    }
  }

  init() {
    // Show/hide button based on scroll position
    window.addEventListener('scroll', () => {
      if (window.pageYOffset > 300) {
        this.button.classList.add('visible');
      } else {
        this.button.classList.remove('visible');
      }
    });

    // Smooth scroll to top when clicked
    this.button.addEventListener('click', () => {
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    });
  }
}

// Lazy Image Loading with Intersection Observer - Performance Optimized
class LazyImageLoader {
  constructor() {
    this.lazyImages = document.querySelectorAll('.lazy-image');
    this.imageObserver = null;
    this.loadedImages = new Set();
    this.loadingImages = new Set();
    this.loadQueue = [];
    this.maxConcurrentLoads = this.getOptimalConcurrency();
    this.currentLoads = 0;

    if (this.lazyImages.length > 0) {
      this.init();
    }
  }

  getOptimalConcurrency() {
    // Detect connection speed and adjust concurrent loads
    if ('connection' in navigator) {
      const connection = navigator.connection;
      if (connection.effectiveType === '4g') {
        return 8; // Fast connection - load 8 images at once
      } else if (connection.effectiveType === '3g') {
        return 6; // Medium connection - load 6 images at once
      } else {
        return 3; // Slow connection - load 3 images at once
      }
    }
    return 6; // Default to 6 concurrent loads
  }

  getOptimalImageSize() {
    // Determine optimal image size based on viewport and device
    const containerWidth = 400; // Portfolio item width
    const devicePixelRatio = window.devicePixelRatio || 1;
    const optimalWidth = Math.ceil(containerWidth * devicePixelRatio);

    // Return appropriate size parameter for the CDN
    if (optimalWidth <= 400) {
      return '400'; // Small size
    } else if (optimalWidth <= 600) {
      return '600'; // Medium size
    } else if (optimalWidth <= 800) {
      return '800'; // Large size
    } else {
      return '1080'; // Full size only for high-DPI displays
    }
  }

  optimizeImageUrl(originalUrl) {
    // Extract the base URL and modify size parameter
    const optimalSize = this.getOptimalImageSize();

    // Replace the size parameter in the URL (e.g., p-1080 -> p-600)
    const optimizedUrl = originalUrl.replace(/p-\d+/, `p-${optimalSize}`);

    // For very fast loading, use an even smaller version first
    const quickLoadUrl = originalUrl.replace(/p-\d+/, 'p-400');

    return {
      quickLoad: quickLoadUrl,
      optimal: optimizedUrl,
      original: originalUrl
    };
  }

  init() {
    // Check for Intersection Observer support
    if ('IntersectionObserver' in window) {
      this.setupIntersectionObserver();
    } else {
      // Fallback for older browsers
      this.setupScrollFallback();
    }
  }

  setupIntersectionObserver() {
    const options = {
      root: null, // Use viewport as root
      rootMargin: '300px', // Start loading 300px before image enters viewport (very aggressive)
      threshold: 0 // Trigger as soon as any part becomes visible
    };

    this.imageObserver = new IntersectionObserver((entries, observer) => {
      const imagesToLoad = [];

      entries.forEach(entry => {
        if (entry.isIntersecting) {
          imagesToLoad.push(entry.target);
          observer.unobserve(entry.target);
        }
      });

      // Process images in batches for better performance
      if (imagesToLoad.length > 0) {
        this.batchLoadImages(imagesToLoad);
      }
    }, options);

    // Start observing all lazy images
    this.lazyImages.forEach(img => {
      this.imageObserver.observe(img);
    });
  }

  setupScrollFallback() {
    // Fallback for browsers without Intersection Observer
    let scrollTimeout;

    const loadImagesOnScroll = () => {
      // Debounce scroll events for better performance
      clearTimeout(scrollTimeout);
      scrollTimeout = setTimeout(() => {
        const imagesToLoad = [];

        this.lazyImages.forEach(img => {
          if (this.isImageInViewport(img) && !this.loadedImages.has(img) && !this.loadingImages.has(img)) {
            imagesToLoad.push(img);
          }
        });

        if (imagesToLoad.length > 0) {
          this.batchLoadImages(imagesToLoad);
        }
      }, 100); // 100ms debounce
    };

    // Load images on scroll and resize
    window.addEventListener('scroll', loadImagesOnScroll, { passive: true });
    window.addEventListener('resize', loadImagesOnScroll, { passive: true });

    // Initial check
    loadImagesOnScroll();
  }

  isImageInViewport(img) {
    const rect = img.getBoundingClientRect();
    const windowHeight = window.innerHeight || document.documentElement.clientHeight;

    return (
      rect.top <= windowHeight + 300 && // 300px margin (very aggressive preloading)
      rect.bottom >= -300 &&
      rect.left <= (window.innerWidth || document.documentElement.clientWidth) &&
      rect.right >= 0
    );
  }

  batchLoadImages(images) {
    // Sort images by priority (visible first, then by distance from viewport)
    const sortedImages = this.prioritizeImages(images);

    // Add to queue and process
    this.loadQueue.push(...sortedImages);
    this.processLoadQueue();
  }

  prioritizeImages(images) {
    return images.sort((a, b) => {
      const rectA = a.getBoundingClientRect();
      const rectB = b.getBoundingClientRect();
      const viewportHeight = window.innerHeight;

      // Calculate distance from viewport center
      const distanceA = Math.abs(rectA.top + rectA.height / 2 - viewportHeight / 2);
      const distanceB = Math.abs(rectB.top + rectB.height / 2 - viewportHeight / 2);

      return distanceA - distanceB; // Closer images first
    });
  }

  processLoadQueue() {
    while (this.loadQueue.length > 0 && this.currentLoads < this.maxConcurrentLoads) {
      const img = this.loadQueue.shift();
      if (img && !this.loadedImages.has(img) && !this.loadingImages.has(img)) {
        this.loadImage(img);
      }
    }
  }

  loadImage(img) {
    if (this.loadedImages.has(img) || this.loadingImages.has(img)) {
      return; // Already loaded or loading
    }

    const dataSrc = img.getAttribute('data-src');
    if (!dataSrc) {
      return; // No data-src attribute
    }

    // Mark as loading
    img.classList.add('loading');
    this.loadingImages.add(img);
    this.currentLoads++;

    // Get optimized image URLs
    const imageUrls = this.optimizeImageUrl(dataSrc);

    // Progressive loading: Start with quick load, then optimal
    this.progressiveLoad(img, imageUrls);
  }

  progressiveLoad(img, imageUrls) {
    const startTime = performance.now();

    // Step 1: Load small version first (blur-up effect)
    const quickLoader = new Image();

    quickLoader.onload = () => {
      // Show small blurred version immediately
      img.src = imageUrls.quickLoad;
      img.classList.add('blur-up');

      const quickLoadTime = performance.now() - startTime;
      console.log(`Quick load completed in ${quickLoadTime.toFixed(0)}ms:`, imageUrls.quickLoad);

      // Step 2: Load optimal quality version
      this.loadOptimalImage(img, imageUrls, startTime);
    };

    quickLoader.onerror = () => {
      // If quick load fails, go straight to optimal
      console.warn('Quick load failed, loading optimal directly');
      this.loadOptimalImage(img, imageUrls, startTime);
    };

    // Start quick load
    quickLoader.src = imageUrls.quickLoad;
  }

  loadOptimalImage(img, imageUrls, startTime) {
    const optimalLoader = new Image();

    optimalLoader.onload = () => {
      // Replace with high-quality version
      img.src = imageUrls.optimal;
      img.classList.remove('loading', 'blur-up');
      img.classList.add('loaded');

      // Remove data-src to prevent reloading
      img.removeAttribute('data-src');

      // Update tracking
      this.loadingImages.delete(img);
      this.loadedImages.add(img);
      this.currentLoads--;

      // Process next images in queue
      this.processLoadQueue();

      const totalTime = performance.now() - startTime;
      console.log(`Image fully loaded in ${totalTime.toFixed(0)}ms:`, imageUrls.optimal);
    };

    optimalLoader.onerror = () => {
      // If optimal fails, try original as fallback
      console.warn('Optimal load failed, trying original');
      this.loadFallbackImage(img, imageUrls.original, startTime);
    };

    // Start optimal load
    optimalLoader.src = imageUrls.optimal;
  }

  loadFallbackImage(img, originalUrl, startTime) {
    const fallbackLoader = new Image();

    fallbackLoader.onload = () => {
      img.src = originalUrl;
      img.classList.remove('loading', 'blur-up');
      img.classList.add('loaded');
      img.removeAttribute('data-src');

      this.loadingImages.delete(img);
      this.loadedImages.add(img);
      this.currentLoads--;
      this.processLoadQueue();

      const totalTime = performance.now() - startTime;
      console.log(`Fallback image loaded in ${totalTime.toFixed(0)}ms:`, originalUrl);
    };

    fallbackLoader.onerror = () => {
      // Complete failure
      img.classList.remove('loading', 'blur-up');
      img.classList.add('error');

      this.loadingImages.delete(img);
      this.currentLoads--;
      this.processLoadQueue();

      console.error('All image loading attempts failed:', originalUrl);
    };

    fallbackLoader.src = originalUrl;
  }

  // Public method to handle new images (for filtering integration)
  observeNewImages() {
    if (this.imageObserver) {
      const newLazyImages = document.querySelectorAll('.lazy-image:not(.loaded):not(.loading):not(.error)');
      newLazyImages.forEach(img => {
        if (!this.loadedImages.has(img) && !this.loadingImages.has(img)) {
          this.imageObserver.observe(img);
        }
      });
    }
  }

  // Public method to force load visible images (for filtering) - optimized
  loadVisibleImages() {
    const imagesToLoad = [];

    this.lazyImages.forEach(img => {
      const portfolioItem = img.closest('.portfolio-item');
      if (portfolioItem && portfolioItem.style.display !== 'none' &&
          portfolioItem.style.opacity !== '0' &&
          !this.loadedImages.has(img) && !this.loadingImages.has(img)) {

        if (this.isImageInViewport(img)) {
          imagesToLoad.push(img);
        }
      }
    });

    if (imagesToLoad.length > 0) {
      console.log(`Loading ${imagesToLoad.length} newly visible images after filtering`);
      this.batchLoadImages(imagesToLoad);
    }
  }

  // Public method to get loading statistics
  getLoadingStats() {
    return {
      total: this.lazyImages.length,
      loaded: this.loadedImages.size,
      loading: this.loadingImages.size,
      queued: this.loadQueue.length,
      remaining: this.lazyImages.length - this.loadedImages.size - this.loadingImages.size
    };
  }
}

// Document ready function
document.addEventListener('DOMContentLoaded', function() {
  // Initialize back to top button
  new BackToTop();

  // Initialize continuous slider if it exists on the page
  const sliderElement = document.querySelector('.image-slider');
  if (sliderElement) {
    new ContinuousSlider();
  }

  // Initialize portfolio filter if it exists on the page
  const portfolioElement = document.querySelector('.portfolio-grid');
  if (portfolioElement) {
    window.portfolioFilter = new PortfolioFilter();

    // Initialize lazy loading for portfolio images
    window.lazyImageLoader = new LazyImageLoader();

    // Integrate lazy loading with portfolio filtering
    if (window.portfolioFilter && window.lazyImageLoader) {
      // Override the original filterPortfolio method to handle lazy loading
      const originalFilterPortfolio = window.portfolioFilter.filterPortfolio.bind(window.portfolioFilter);

      window.portfolioFilter.filterPortfolio = function(category) {
        // Call original filtering logic
        originalFilterPortfolio(category);

        // After filtering, check for newly visible images that need loading
        setTimeout(() => {
          window.lazyImageLoader.loadVisibleImages();
          window.lazyImageLoader.observeNewImages();

          // Log performance stats
          const stats = window.lazyImageLoader.getLoadingStats();
          console.log('Lazy loading stats after filtering:', stats);
        }, 350); // Wait for filter transition to complete
      };
    }
  } else {
    // Initialize lazy loading even if no portfolio (for other pages)
    const lazyImages = document.querySelectorAll('.lazy-image');
    if (lazyImages.length > 0) {
      window.lazyImageLoader = new LazyImageLoader();
    }
  }

  console.log('Professional Photography Studio website loaded');
});
