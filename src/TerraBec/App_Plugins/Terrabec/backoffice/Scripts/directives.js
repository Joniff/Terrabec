angular.module('umbraco.directives').directive('terrabecIframe',
	['$compile',
	function ($compile) {
		return {
			restrict: 'E',
			template: '<iframe></iframe>',
			scope: {
				iframeTemplate: '=',
				iframeContext: '='
			},
			link: function ($scope, $element, $attrs) {

				var setScope = function () {
					angular.forEach($scope.iframeContext, function (value, key) {
						$scope[key] = value;
					});
					setContextWatchers();
				};

				var render = function () {
					$compile($element
						.find('iframe').contents()
						.find('body').html($scope.iframeTemplate)
						.contents()
					)($scope);
				};

				var setContextWatchers = function () {
					angular.forEach($scope.iframeContext, function (value, key) {
						$scope.$watch(
							function ($scope) {
								return $scope.iframeContext[key];
							},
							function () {
								$scope[key] = $scope.iframeContext[key];
							});
					});
				};

				$scope.$watch('iframeTemplate', () => {
					setScope();
					render('template');
				});
			}
		}
	}
]);
angular.module('umbraco.directives').directive('terrabecDynamicHtml',
	['$compile',
	function ($compile) {
		return {
			restrict: 'E',
			scope: {
				contents: '='
			},
			link: function ($scope, $element, $attrs) {
				let element = $element;
				$scope.$watch('contents', function () {
					let newElement = $compile($scope.contents)($scope);
					element.replaceWith(newElement);
					element = newElement;
				});
			}
		};
	}
]);
